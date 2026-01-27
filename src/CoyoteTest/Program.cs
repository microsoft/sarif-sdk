// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace CoyoteTest;

/// <summary>
/// Tests to verify the thread-safety fix for the concurrency bug documented in work item 2250448.
/// 
/// The bug manifested as:
///   System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
///   at System.Collections.Generic.Dictionary`2.Enumerator.MoveNext()
///   at Newtonsoft.Json.Serialization.JsonSerializerInternalWriter.SerializeDictionary(...)
///   at Microsoft.CodeAnalysis.Sarif.Writers.ResultLogJsonWriter.WriteTool(Tool tool)
///   at Microsoft.CodeAnalysis.Sarif.Writers.SarifLogger.Dispose()
/// 
/// The fix uses Interlocked counters to track in-flight Log() operations and SpinWait
/// in Dispose() to wait for all operations to complete before serialization.
/// </summary>
public static class SarifLoggerConcurrencyTests
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Thread-Safety Fix Verification Tests for SarifLogger");
        Console.WriteLine("=" + new string('=', 70));
        Console.WriteLine();
        Console.WriteLine("This fix can be controlled via the --thread-safe-logging option.");
        Console.WriteLine("In code: new SarifLogger(..., threadSafeLoggingEnabled: true/false)");
        Console.WriteLine();

        // PHASE 1: Test WITHOUT fix to demonstrate the bug exists
        Console.WriteLine(">>> PHASE 1: Testing WITHOUT fix (threadSafeLoggingEnabled: false)");
        Console.WriteLine("    Expected: Tests should FAIL with 'Collection was modified' errors");
        Console.WriteLine();
        
        await RunRealWorldStressTestAsync(nameof(StressTest_ConcurrentLogAndDispose), () => StressTest_ConcurrentLogAndDispose(threadSafe: false), iterations: 20, expectFailure: true);
        await RunRealWorldStressTestAsync(nameof(StressTest_ManyThreadsLogThenDispose), () => StressTest_ManyThreadsLogThenDispose(threadSafe: false), iterations: 10, expectFailure: true);

        // PHASE 2: Test WITH fix to demonstrate it works
        Console.WriteLine();
        Console.WriteLine(">>> PHASE 2: Testing WITH fix (threadSafeLoggingEnabled: true)");
        Console.WriteLine("    Expected: Tests should PASS");
        Console.WriteLine();
        
        await RunRealWorldStressTestAsync(nameof(StressTest_ConcurrentLogAndDispose), () => StressTest_ConcurrentLogAndDispose(threadSafe: true), iterations: 100, expectFailure: false);
        await RunRealWorldStressTestAsync(nameof(StressTest_ManyThreadsLogThenDispose), () => StressTest_ManyThreadsLogThenDispose(threadSafe: true), iterations: 50, expectFailure: false);
        await RunRealWorldStressTestAsync(nameof(StressTest_DisposeDuringActiveLogs), () => StressTest_DisposeDuringActiveLogs(threadSafe: true), iterations: 100, expectFailure: false);
        await RunRealWorldStressTestAsync(nameof(StressTest_DisposeRejectsNewLogs), () => StressTest_DisposeRejectsNewLogs(threadSafe: true), iterations: 50, expectFailure: false);

        Console.WriteLine("\n" + new string('=', 70));
        Console.WriteLine("All tests completed.");
    }

    private static async Task RunRealWorldStressTestAsync(string testName, Func<Task> test, int iterations, bool expectFailure = false)
    {
        Console.WriteLine($"\n--- Running: {testName} ({iterations} iterations) ---");
        
        int failures = 0;
        string? lastError = null;

        for (int i = 0; i < iterations; i++)
        {
            try
            {
                await test();
            }
            catch (Exception ex)
            {
                failures++;
                lastError = ex.Message;
            }
        }

        if (expectFailure)
        {
            if (failures > 0)
            {
                Console.WriteLine($"✓ EXPECTED FAILURE: {failures}/{iterations} iterations failed (bug confirmed!)");
            }
            else
            {
                Console.WriteLine($"✗ UNEXPECTED SUCCESS: Test should have failed but all iterations passed");
            }
        }
        else
        {
            if (failures > 0)
            {
                Console.WriteLine($"✗ FAILED: {failures}/{iterations} iterations failed (fix not working!)");
                Console.WriteLine($"  Last error: {lastError}");
            }
            else
            {
                Console.WriteLine($"✓ PASSED: All {iterations} iterations succeeded (fix working!)");
            }
        }
    }

    /// <summary>
    /// Stress test: Multiple threads call Log() while another thread calls Dispose().
    /// Before the fix, this would cause "Collection was modified" exception.
    /// After the fix, Dispose() waits for all Log() calls to complete.
    /// </summary>
    public static async Task StressTest_ConcurrentLogAndDispose(bool threadSafe)
    {
        var run = new Run { Tool = Tool.CreateFromAssemblyData() };
        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: run,
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail,
            threadSafeLoggingEnabled: threadSafe);

        logger.AnalysisStarted();

        var cts = new CancellationTokenSource();
        var logTasks = new List<Task>();
        var barrier = new Barrier(5); // 4 log threads + 1 dispose thread

        // Start 4 threads that continuously log results with DIFFERENT rules
        // This causes _run.Tool.Driver.Rules to be modified during Log()
        for (int t = 0; t < 4; t++)
        {
            int threadId = t;
            logTasks.Add(Task.Run(() =>
            {
                barrier.SignalAndWait(); // Synchronize start
                int count = 0;
                while (!cts.Token.IsCancellationRequested && count < 100)
                {
                    try
                    {
                        // Each iteration creates a new rule - this modifies Tool.Driver.Rules
                        var rule = new ReportingDescriptor { Id = $"RULE-T{threadId}-{count}" };
                        var result = new Result
                        {
                            RuleId = rule.Id,
                            Message = new Message { Text = $"Thread {threadId} result {count}" },
                            Level = FailureLevel.Warning
                        };
                        logger.Log(rule, result, null);
                        count++;
                    }
                    catch (ObjectDisposedException)
                    {
                        // Expected after dispose - this is correct behavior
                        break;
                    }
                }
            }));
        }

        // Wait a tiny bit then dispose while logs are still running
        var disposeTask = Task.Run(async () =>
        {
            barrier.SignalAndWait(); // Synchronize start
            await Task.Delay(1); // Let some logs start
            logger.AnalysisStopped(RuntimeConditions.None);
            logger.Dispose(); // This should wait for in-flight Log() calls
        });

        await disposeTask;
        cts.Cancel();
        await Task.WhenAll(logTasks);

        // If we got here without exception, the fix is working!
    }

    /// <summary>
    /// Stress test: Many threads log results, then dispose is called.
    /// Verifies that all logged results are captured.
    /// </summary>
    public static async Task StressTest_ManyThreadsLogThenDispose(bool threadSafe)
    {
        var run = new Run { Tool = Tool.CreateFromAssemblyData() };
        using var memStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memStream, leaveOpen: true);
        
        var logger = new SarifLogger(
            streamWriter,
            run: run,
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail,
            closeWriterOnDispose: false,
            threadSafeLoggingEnabled: threadSafe);

        logger.AnalysisStarted();

        int totalLogs = 0;
        var logTasks = new List<Task>();
        var countdown = new CountdownEvent(8);

        // Start 8 threads that each log 50 results with different rules
        for (int t = 0; t < 8; t++)
        {
            int threadId = t;
            logTasks.Add(Task.Run(() =>
            {
                countdown.Signal();
                countdown.Wait(); // All threads start together
                
                for (int i = 0; i < 50; i++)
                {
                    // Each creates a unique rule - this modifies Tool.Driver.Rules
                    var rule = new ReportingDescriptor { Id = $"RULE-T{threadId}-{i}" };
                    var result = new Result
                    {
                        RuleId = rule.Id,
                        Message = new Message { Text = $"T{threadId}-{i}" },
                        Level = FailureLevel.Warning
                    };
                    logger.Log(rule, result, null);
                    Interlocked.Increment(ref totalLogs);
                }
            }));
        }

        await Task.WhenAll(logTasks);
        
        logger.AnalysisStopped(RuntimeConditions.None);
        logger.Dispose();

        // Verify all results were written
        streamWriter.Flush();
        memStream.Position = 0;
        var sarifLog = SarifLog.Load(memStream);
        
        if (sarifLog.Runs[0].Results?.Count != totalLogs)
        {
            throw new Exception($"Expected {totalLogs} results, got {sarifLog.Runs[0].Results?.Count}");
        }
    }

    /// <summary>
    /// Stress test: Dispose is called while Log() calls are actively in progress.
    /// The fix ensures Dispose() waits for all in-flight operations.
    /// </summary>
    public static async Task StressTest_DisposeDuringActiveLogs(bool threadSafe)
    {
        var run = new Run { Tool = Tool.CreateFromAssemblyData() };
        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: run,
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail,
            threadSafeLoggingEnabled: threadSafe);

        logger.AnalysisStarted();

        var rule = new ReportingDescriptor { Id = "TEST001" };
        var started = new ManualResetEventSlim(false);
        int successfulLogs = 0;
        int rejectedLogs = 0;

        // Thread that continuously logs
        var logTask = Task.Run(() =>
        {
            started.Set();
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    var result = new Result
                    {
                        RuleId = rule.Id,
                        Message = new Message { Text = $"Result {i}" },
                        Level = FailureLevel.Warning
                    };
                    logger.Log(rule, result, null);
                    Interlocked.Increment(ref successfulLogs);
                }
                catch (ObjectDisposedException)
                {
                    Interlocked.Increment(ref rejectedLogs);
                }
            }
        });

        // Wait for logging to start, then dispose
        started.Wait();
        logger.AnalysisStopped(RuntimeConditions.None);
        logger.Dispose();

        await logTask;

        // Both successful logs and rejected logs are valid outcomes
        // The key is no crash occurred
        Console.Write($" [logged={successfulLogs}, rejected={rejectedLogs}]");
    }

    /// <summary>
    /// Test: Verify that Log() throws ObjectDisposedException after Dispose().
    /// </summary>
    public static async Task StressTest_DisposeRejectsNewLogs(bool threadSafe)
    {
        var run = new Run { Tool = Tool.CreateFromAssemblyData() };
        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: run,
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail,
            threadSafeLoggingEnabled: threadSafe);

        logger.AnalysisStarted();
        logger.AnalysisStopped(RuntimeConditions.None);
        logger.Dispose();

        var rule = new ReportingDescriptor { Id = "TEST001" };
        var result = new Result
        {
            RuleId = rule.Id,
            Message = new Message { Text = "After dispose" },
            Level = FailureLevel.Warning
        };

        bool threwDisposed = false;
        try
        {
            logger.Log(rule, result, null);
        }
        catch (ObjectDisposedException)
        {
            threwDisposed = true;
        }

        if (!threwDisposed)
        {
            throw new Exception("Expected ObjectDisposedException when logging after Dispose()");
        }

        await Task.CompletedTask;
    }
}
