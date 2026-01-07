// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Microsoft.Coyote.SystematicTesting;

public class CoyoteTest
{
    public static async Task Main()
    {
        await SarifLogger_DisposeDuringConcurrentToolPropertiesMutation_ShouldNotThrow();
        await SarifLogger_DisposeDuringConcurrentInvocationEnvironmentMutation_ShouldNotThrow();
        await SarifLogger_DisposeDuringToolComponentPropertiesMutation_ShouldNotThrow();
        await SarifLogger_RepeatedDisposeCyclesWithConcurrentMutations_ShouldNotThrow();
        await SarifLogger_DisposeDuringConcurrentToolMutation_ShouldNotThrow();
    }

    [Test]
    public static async Task SarifLogger_DisposeDuringConcurrentToolPropertiesMutation_ShouldNotThrow()
    {
        // Matches the reported diagram:
        //  - Thread A: SarifLogger.Dispose -> CompleteRun -> WriteTool -> serializes Tool.Properties
        //  - Thread B: Mutates Tool.Properties
        //
        // In SARIF, Tool derives from PropertyBagHolder and stores its property bag in an internal
        // Dictionary<string, SerializedPropertyInfo> that Json.NET enumerates during serialization.

        var tool = Tool.CreateFromAssemblyData();

        // Ensure Tool.Properties dictionary exists.
        tool.SetProperty("seed", "0");

        using var writer = new StringWriter();
        var logger = new SarifLogger(
            writer,
            run: new Run { Tool = tool },
            levels: BaseLogger.ErrorWarningNote,
            kinds: BaseLogger.Fail);

        // Use a handshake to maximize chance of racing during Dispose.
        var startedMutating = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Exception observed = null;

        var mutator = Task.Run(async () =>
        {
            int i = 0;
            startedMutating.TrySetResult(true);

            while (!stop.Task.IsCompleted)
            {
                // Mutate Tool.Properties through the public SetProperty API.
                // This ultimately replaces/updates entries in the internal dictionary.
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

        if (observed != null)
        {
            throw new Exception("Observed exception during SarifLogger.Dispose (Tool.Properties race).", observed);
        }
    }

    [Test]
    public static async Task SarifLogger_DisposeDuringConcurrentInvocationEnvironmentMutation_ShouldNotThrow()
    {
        // The reported stack trace shows a Dictionary enumerator failing inside Json.NET.
        // One hot dictionary in SARIF is Invocation.EnvironmentVariables.
        // This test races dictionary writes with serialization during Dispose.

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
                // Intentionally mutate the dictionary while serialization might enumerate it.
                env[$"K{i++:D4}"] = "V";
                await Task.Yield();
            }
        });

        Exception observed = null;
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

        if (observed != null)
        {
            throw new Exception("Observed exception during SarifLogger.Dispose (Invocation.EnvironmentVariables race).", observed);
        }
    }

    [Test]
    public static async Task SarifLogger_DisposeDuringToolComponentPropertiesMutation_ShouldNotThrow()
    {
        // Use a public dictionary-backed property on Run.
        // This is closer to the reported failure (Dictionary enumerator during Json.NET serialization).

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

        Exception observed = null;
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

        if (observed != null)
        {
            throw new Exception("Observed exception during SarifLogger.Dispose (Run.OriginalUriBaseIds race).", observed);
        }
    }

    [Test]
    public static async Task SarifLogger_RepeatedDisposeCyclesWithConcurrentMutations_ShouldNotThrow()
    {
        // If the bug is rare, a single dispose might not be enough for Coyote to hit the interleaving.
        // This test creates multiple logger instances (fresh writers), while a background task mutates
        // shared state that will be serialized during Dispose.

        var tool = Tool.CreateFromAssemblyData();
        tool.Driver ??= new ToolComponent { Name = "Sarif.Driver" };
        tool.Driver.Rules ??= new List<ReportingDescriptor>();

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

        Exception observed = null;

        // Keep the loop modest; Coyote will explore schedules, not brute force.
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

        if (observed != null)
        {
            throw new Exception("Observed exception during repeated SarifLogger.Dispose cycles.", observed);
        }
    }


    [Test]
    public static async Task SarifLogger_DisposeDuringConcurrentToolMutation_ShouldNotThrow()
    {
        // A lower-level reproducer that doesn't involve MultithreadedAnalyzeCommandBase.
        // It tries to create the same failure shape: logger Dispose serializes `Run.Tool`
        // while another task mutates nested collections.

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

        Exception observed = null;
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

        if (observed != null)
        {
            throw new Exception("Observed exception during SarifLogger.Dispose.", observed);
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"sarif-coyote-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }
}
