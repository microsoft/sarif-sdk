// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Channels;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.Coyote;
using Microsoft.Coyote.Specifications;
using Microsoft.Coyote.SystematicTesting;

namespace CoyoteTest;

/// <summary>
/// Coyote tests to reproduce the concurrency bug documented in work item 2250448.
/// 
/// The bug manifests as:
///   System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
///   at System.Collections.Generic.Dictionary`2.Enumerator.MoveNext()
///   at Newtonsoft.Json.Serialization.JsonSerializerInternalWriter.SerializeDictionary(...)
///   at Microsoft.CodeAnalysis.Sarif.Writers.ResultLogJsonWriter.WriteTool(Tool tool)
///   at Microsoft.CodeAnalysis.Sarif.Writers.SarifLogger.Dispose()
/// 
/// The race condition occurs when:
/// - Thread A: SarifLogger.Dispose() -> CompleteRun() -> WriteTool() -> serializes Tool.Properties
/// - Thread B: Analysis thread modifies Tool.Properties or other shared collections
/// </summary>
public static class SarifLoggerConcurrencyTests
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Running Coyote systematic tests for SarifLogger concurrency bug...");
        Console.WriteLine("=" + new string('=', 70));

        // Run all test methods
        await RunTestAsync(nameof(DisposeDuringConcurrentToolPropertiesMutation), DisposeDuringConcurrentToolPropertiesMutation);
        await RunTestAsync(nameof(DisposeDuringConcurrentInvocationEnvironmentMutation), DisposeDuringConcurrentInvocationEnvironmentMutation);
        await RunTestAsync(nameof(DisposeDuringToolComponentPropertiesMutation), DisposeDuringToolComponentPropertiesMutation);
        await RunTestAsync(nameof(DisposeDuringConcurrentToolMutation), DisposeDuringConcurrentToolMutation);
        await RunTestAsync(nameof(DisposeDuringConcurrentResultsMutation), DisposeDuringConcurrentResultsMutation);
        await RunTestAsync(nameof(SimulateMultithreadedAnalyzeCommandScenario), SimulateMultithreadedAnalyzeCommandScenario);
        await RunTestAsync(nameof(RepeatedDisposeCyclesWithConcurrentMutations), RepeatedDisposeCyclesWithConcurrentMutations);

        Console.WriteLine("=" + new string('=', 70));
        Console.WriteLine("All Coyote tests completed.");
    }

    private static async Task RunTestAsync(string testName, Func<Task> test)
    {
        Console.WriteLine($"\n--- Running: {testName} ---");

        var configuration = Configuration.Create()
            .WithTestingIterations(1000)
            .WithMaxSchedulingSteps(1000);

        var engine = TestingEngine.Create(configuration, test);
        engine.Run();

        Console.WriteLine($"Iterations: {engine.TestReport.NumOfFoundBugs} bugs found");

        if (engine.TestReport.NumOfFoundBugs > 0)
        {
            Console.WriteLine("BUG FOUND! Reproducible trace available.");
            Console.WriteLine($"Error: {engine.TestReport.BugReports.FirstOrDefault()}");

            // Generate replay trace
            string traceFile = $"{testName}_replay.schedule";
            engine.TryEmitReports(".", testName, out _);
            Console.WriteLine($"Replay trace saved to: {traceFile}");
        }
        else
        {
            Console.WriteLine("No bugs found in this test.");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Test 1: Race between Dispose serializing Tool.Properties and concurrent mutation.
    /// 
    /// Matches the reported diagram:
    ///  - Thread A: SarifLogger.Dispose -> CompleteRun -> WriteTool -> serializes Tool.Properties
    ///  - Thread B: Mutates Tool.Properties
    /// </summary>
    [Microsoft.Coyote.SystematicTesting.Test]
    public static async Task DisposeDuringConcurrentToolPropertiesMutation()
    {
        var tool = Tool.CreateFromAssemblyData();
        tool.SetProperty("seed", "0");

        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: new Run { Tool = tool },
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail);

        var startedMutating = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Exception? observed = null;

        var mutator = Task.Run(async () =>
        {
            int i = 0;
            startedMutating.TrySetResult(true);
            while (!stop.Task.IsCompleted)
            {
                // Mutate Tool.Properties through the public SetProperty API.
                tool.SetProperty($"k{i++:D4}", "v");
                await Task.Yield();
            }
        });

        await startedMutating.Task;

        var disposer = Task.Run(() =>
        {
            try
            {
                logger.AnalysisStarted();
                logger.AnalysisStopped(RuntimeConditions.None);
                logger.Dispose();
            }
            catch (Exception ex)
            {
                observed = ex;
            }
        });

        await disposer;
        stop.TrySetResult(true);
        await mutator;

        Specification.Assert(observed == null, $"Observed exception during SarifLogger.Dispose (Tool.Properties race): {observed?.Message}");
    }

    /// <summary>
    /// Test 2: Race between Dispose and concurrent Invocation.EnvironmentVariables mutation.
    /// The reported stack trace shows a Dictionary enumerator failing inside Json.NET.
    /// </summary>
    [Microsoft.Coyote.SystematicTesting.Test]
    public static async Task DisposeDuringConcurrentInvocationEnvironmentMutation()
    {
        var run = new Run
        {
            Tool = Tool.CreateFromAssemblyData(),
            Invocations = new List<Invocation>
            {
                new Invocation
                {
                    EnvironmentVariables = new Dictionary<string, string>()
                }
            }
        };

        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: run,
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail);

        using var cts = new CancellationTokenSource();
        IDictionary<string, string> env = run.Invocations[0].EnvironmentVariables;

        var mutator = Task.Run(async () =>
        {
            int i = 0;
            while (!cts.IsCancellationRequested)
            {
                env[$"K{i++:D4}"] = "V";
                await Task.Yield();
            }
        });

        Exception? observed = null;

        var disposer = Task.Run(() =>
        {
            try
            {
                logger.AnalysisStarted();
                logger.AnalysisStopped(RuntimeConditions.None);
                logger.Dispose();
            }
            catch (Exception ex)
            {
                observed = ex;
            }
        });

        await disposer;
        cts.Cancel();
        await mutator;

        Specification.Assert(observed == null, $"Observed exception during SarifLogger.Dispose (Invocation.EnvironmentVariables race): {observed?.Message}");
    }

    /// <summary>
    /// Test 3: Race with Run.OriginalUriBaseIds dictionary mutation.
    /// </summary>
    [Microsoft.Coyote.SystematicTesting.Test]
    public static async Task DisposeDuringToolComponentPropertiesMutation()
    {
        var run = new Run
        {
            Tool = Tool.CreateFromAssemblyData(),
            OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>()
        };

        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: run,
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail);

        using var cts = new CancellationTokenSource();
        IDictionary<string, ArtifactLocation> uriBaseIds = run.OriginalUriBaseIds;

        var mutator = Task.Run(async () =>
        {
            int i = 0;
            while (!cts.IsCancellationRequested)
            {
                uriBaseIds[$"BASE{i++:D4}"] = new ArtifactLocation { Uri = new Uri($"file:///C:/tmp/{i}.txt") };
                await Task.Yield();
            }
        });

        Exception? observed = null;

        var disposer = Task.Run(() =>
        {
            try
            {
                logger.AnalysisStarted();
                logger.AnalysisStopped(RuntimeConditions.None);
                logger.Dispose();
            }
            catch (Exception ex)
            {
                observed = ex;
            }
        });

        await disposer;
        cts.Cancel();
        await mutator;

        Specification.Assert(observed == null, $"Observed exception during SarifLogger.Dispose (Run.OriginalUriBaseIds race): {observed?.Message}");
    }

    /// <summary>
    /// Test 4: Race between Dispose and Tool.Driver.Rules list mutation.
    /// </summary>
    [Microsoft.Coyote.SystematicTesting.Test]
    public static async Task DisposeDuringConcurrentToolMutation()
    {
        var tool = Tool.CreateFromAssemblyData();
        tool.Driver ??= new ToolComponent { Name = "Sarif.Driver" };
        tool.Driver.Rules ??= new List<ReportingDescriptor>();

        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: new Run { Tool = tool },
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail);

        using var cts = new CancellationTokenSource();

        var mutator = Task.Run(async () =>
        {
            int i = 0;
            while (!cts.IsCancellationRequested)
            {
                tool.Driver.Rules.Add(new ReportingDescriptor { Id = $"TST{i++:D4}", Name = "rule" });
                await Task.Yield();
            }
        });

        Exception? observed = null;

        var disposer = Task.Run(() =>
        {
            try
            {
                logger.AnalysisStarted();
                logger.AnalysisStopped(RuntimeConditions.None);
                logger.Dispose();
            }
            catch (Exception ex)
            {
                observed = ex;
            }
        });

        await disposer;
        cts.Cancel();
        await mutator;

        Specification.Assert(observed == null, $"Observed exception during SarifLogger.Dispose: {observed?.Message}");
    }

    /// <summary>
    /// Test 5: Race between Dispose and concurrent Results list mutation.
    /// This simulates logging results while Dispose is serializing.
    /// </summary>
    [Microsoft.Coyote.SystematicTesting.Test]
    public static async Task DisposeDuringConcurrentResultsMutation()
    {
        var run = new Run
        {
            Tool = Tool.CreateFromAssemblyData(),
            Results = new List<Result>()
        };

        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: run,
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail);

        using var cts = new CancellationTokenSource();

        var mutator = Task.Run(async () =>
        {
            int i = 0;
            while (!cts.IsCancellationRequested)
            {
                var result = new Result
                {
                    RuleId = $"TST{i++:D4}",
                    Message = new Message { Text = $"Test result {i}" },
                    Level = FailureLevel.Warning
                };
                run.Results.Add(result);
                await Task.Yield();
            }
        });

        Exception? observed = null;

        var disposer = Task.Run(() =>
        {
            try
            {
                logger.AnalysisStarted();
                logger.AnalysisStopped(RuntimeConditions.None);
                logger.Dispose();
            }
            catch (Exception ex)
            {
                observed = ex;
            }
        });

        await disposer;
        cts.Cancel();
        await mutator;

        Specification.Assert(observed == null, $"Observed exception during SarifLogger.Dispose (Results race): {observed?.Message}");
    }

    /// <summary>
    /// Test 6: Simulates the MultithreadedAnalyzeCommandBase scenario more closely.
    /// 
    /// In the real code:
    /// - Multiple analysis threads write to thread-local CachingLoggers
    /// - LogResultsAsync transfers cached results to global logger under a lock
    /// - Main thread calls Dispose() which serializes without acquiring that lock
    /// 
    /// The race happens because Dispose() doesn't wait for LogResultsAsync to complete.
    /// </summary>
    [Microsoft.Coyote.SystematicTesting.Test]
    public static async Task SimulateMultithreadedAnalyzeCommandScenario()
    {
        var tool = Tool.CreateFromAssemblyData();
        tool.Driver ??= new ToolComponent { Name = "Sarif.Driver" };
        tool.Driver.Rules ??= new List<ReportingDescriptor>();

        var run = new Run
        {
            Tool = tool,
            Results = new List<Result>(),
            Artifacts = new List<Artifact>()
        };

        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: run,
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail);

        // Simulate the _resultsWritingChannel from MultithreadedAnalyzeCommandBase
        var resultsChannel = Channel.CreateUnbounded<int>();
        var lockObject = new object();

        // Simulate scan workers completing
        var scanWorkersComplete = new TaskCompletionSource<bool>();
        int filesScanned = 0;
        const int totalFiles = 50;

        Exception? observed = null;

        // Simulate multiple analysis threads
        var analysisThreads = new Task[4];
        for (int threadId = 0; threadId < analysisThreads.Length; threadId++)
        {
            int tid = threadId;
            analysisThreads[threadId] = Task.Run(async () =>
            {
                while (true)
                {
                    int fileIndex = Interlocked.Increment(ref filesScanned);
                    if (fileIndex > totalFiles) break;

                    // Simulate analysis producing results
                    await Task.Yield();

                    // Signal that this file is ready for logging
                    await resultsChannel.Writer.WriteAsync(fileIndex);
                }
            });
        }

        // Simulate LogResultsAsync - single-threaded consumer
        var logResultsTask = Task.Run(async () =>
        {
            await foreach (var fileIndex in resultsChannel.Reader.ReadAllAsync())
            {
                // This lock exists in the real code
                lock (lockObject)
                {
                    // Simulate LogCachingLogger modifying shared state
                    run.Results.Add(new Result
                    {
                        RuleId = "TST001",
                        Message = new Message { Text = $"Result for file {fileIndex}" }
                    });

                    // Modify Tool properties (this is what causes the bug)
                    tool.SetProperty($"file_{fileIndex}", fileIndex.ToString());
                }

                await Task.Yield();
            }
        });

        // Wait for scan workers to complete, then signal channel completion
        _ = Task.WhenAll(analysisThreads).ContinueWith(_ => resultsChannel.Writer.Complete());

        // Simulate the main thread calling Dispose before LogResultsAsync completes
        // This is the race condition!
        var disposeTask = Task.Run(async () =>
        {
            // Wait a bit to let some analysis happen
            await Task.Yield();

            try
            {
                logger.AnalysisStarted();
                logger.AnalysisStopped(RuntimeConditions.None);

                // THE BUG: Dispose() is called without waiting for logResultsTask
                // and without acquiring the lock that protects modifications
                logger.Dispose();
            }
            catch (Exception ex)
            {
                observed = ex;
            }
        });

        await Task.WhenAll(analysisThreads);
        await disposeTask;
        resultsChannel.Writer.TryComplete();
        await logResultsTask;

        Specification.Assert(observed == null, $"Observed exception simulating MultithreadedAnalyzeCommand scenario: {observed?.Message}");
    }

    /// <summary>
    /// Test 7: Multiple dispose cycles with concurrent mutations.
    /// If the bug is rare, a single dispose might not trigger the interleaving.
    /// </summary>
    [Microsoft.Coyote.SystematicTesting.Test]
    public static async Task RepeatedDisposeCyclesWithConcurrentMutations()
    {
        var tool = Tool.CreateFromAssemblyData();
        tool.Driver ??= new ToolComponent { Name = "Sarif.Driver" };
        tool.Driver.Rules ??= new List<ReportingDescriptor>();

        using var cts = new CancellationTokenSource();
        Exception? observed = null;

        var mutator = Task.Run(async () =>
        {
            int i = 0;
            while (!cts.IsCancellationRequested)
            {
                tool.Driver.Rules.Add(new ReportingDescriptor { Id = $"TST{i++:D4}", Name = "rule" });
                await Task.Yield();
            }
        });

        // Run multiple dispose cycles
        for (int iter = 0; iter < 10 && observed == null; iter++)
        {
            using var writer = new StringWriter();
            var logger = new SarifLogger(
                writer,
                run: new Run { Tool = tool },
                levels: BaseLogger.ErrorWarningNote,
                kinds: BaseLogger.Fail);

            try
            {
                logger.AnalysisStarted();
                logger.AnalysisStopped(RuntimeConditions.None);
                logger.Dispose();
            }
            catch (Exception ex)
            {
                observed = ex;
            }

            await Task.Yield();
        }

        cts.Cancel();
        await mutator;

        Specification.Assert(observed == null, $"Observed exception during repeated SarifLogger.Dispose cycles: {observed?.Message}");
    }
}
