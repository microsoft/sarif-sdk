// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class MultithreadedAnalyzeCommandBaseAsyncTests
    {
        private const int FAILURE = CommandBase.FAILURE;
        private const int SUCCESS = CommandBase.SUCCESS;

        private static readonly TimeSpan PipelineTimeout = TimeSpan.FromMinutes(2);

        [Fact]
        public async Task RunAsync_MatchesRunExitCodeForSuccessfulAnalysis()
        {
            (RunOutcome synchronous, RunOutcome asynchronous) = await AnalyzeBothWaysAsync(AnalyzeThisAssembly);

            synchronous.ExitCode.Should().Be(SUCCESS);
            asynchronous.ExitCode.Should().Be(synchronous.ExitCode);
        }

        [Fact]
        public async Task RunAsync_MatchesRunRuntimeErrorsForSuccessfulAnalysis()
        {
            (RunOutcome synchronous, RunOutcome asynchronous) = await AnalyzeBothWaysAsync(AnalyzeThisAssembly);

            synchronous.RuntimeErrors.Should().Be(RuntimeConditions.None);
            asynchronous.RuntimeErrors.Should().Be(synchronous.RuntimeErrors);
        }

        [Fact]
        public async Task RunAsync_MatchesRunResultsForSuccessfulAnalysis()
        {
            (RunOutcome synchronous, RunOutcome asynchronous) = await AnalyzeBothWaysAsync(AnalyzeThisAssembly);

            synchronous.Run.Results.Count.Should().Be((int)TestRule.ErrorsCount.DefaultValue());
            asynchronous.Run.Results.Count.Should().Be(synchronous.Run.Results.Count);
            asynchronous.Run.Results.Select(result => result.RuleId)
                        .Should().Equal(synchronous.Run.Results.Select(result => result.RuleId));
        }

        [Fact]
        public async Task RunAsync_MatchesRunWhenThereAreNoValidAnalysisTargets()
        {
            (RunOutcome synchronous, RunOutcome asynchronous) = await AnalyzeBothWaysAsync(AnalyzeNothing);

            synchronous.ExitCode.Should().Be(FAILURE);
            synchronous.RuntimeErrors.Should().Be(RuntimeConditions.NoValidAnalysisTargets);

            asynchronous.ExitCode.Should().Be(synchronous.ExitCode);
            asynchronous.RuntimeErrors.Should().Be(synchronous.RuntimeErrors);
        }

        [Fact]
        public async Task RunAsync_DisposesGlobalContextItCreated()
        {
            (int _, TestAnalysisContext context) =
                await CreateCommand().RunAsync(CreateOptions(), globalContext: null);

            context.Should().NotBeNull();
            context.Disposed.Should().BeTrue();
        }

        [Fact]
        public async Task RunAsync_DisposesGlobalContextSuppliedByCaller()
        {
            var suppliedContext = new TestAnalysisContext();

            (int _, TestAnalysisContext context) =
                await CreateCommand().RunAsync(CreateOptions(), suppliedContext);

            context.Should().BeSameAs(suppliedContext);
            suppliedContext.Disposed.Should().BeTrue();
        }

        [Fact]
        public async Task RunAsync_HonorsRichReturnCode()
        {
            TestAnalyzeOptions options = CreateOptions(AnalyzeNothing);
            options.RichReturnCode = true;

            (int exitCode, TestAnalysisContext context) = await CreateCommand().RunAsync(options, globalContext: null);

            context.RuntimeErrors.Should().NotBe(RuntimeConditions.None);
            exitCode.Should().Be((int)context.RuntimeErrors);
        }

        [Fact]
        public async Task RunAsync_ReturnsToCallerBeforeAnalysisCompletes()
        {
            var command = new GatedAnalyzeCommand();

            Task<(int ExitCode, TestAnalysisContext GlobalContext)> runTask =
                command.RunAsync(CreateOptions(), globalContext: null);

            await WaitForAsync(command.AnalysisStarted.Task);

            // The gate is still shut, so the analysis cannot have finished. A pipeline that
            // blocked its caller could not have handed this task back to us at all.
            runTask.IsCompleted.Should().BeFalse();

            command.OpenGate();

            (int exitCode, TestAnalysisContext context) = await runTask;
            exitCode.Should().Be(SUCCESS);
            context.RuntimeErrors.Should().Be(RuntimeConditions.None);
        }

        [Fact]
        public async Task RunAsync_ReportsTimeoutWhenAnalysisExceedsBudget()
        {
            // The gate is never opened, so analysis cannot complete and the timeout must win.
            var command = new GatedAnalyzeCommand();

            TestAnalyzeOptions options = CreateOptions();
            options.TimeoutInSeconds = 0;

            (int exitCode, TestAnalysisContext context) = await command.RunAsync(options, globalContext: null);

            exitCode.Should().Be(FAILURE);
            context.RuntimeErrors.Should().Be(RuntimeConditions.AnalysisTimedOut);
        }

        [Fact]
        public void Run_DispatchesToSynchronousAnalyzeTargets()
        {
            var command = new DispatchRecordingCommand();

            TestAnalysisContext context = null;
            command.Run(CreateOptions(), ref context);

            command.SynchronousAnalyzeTargetsCount.Should().Be(1);
            command.AsynchronousAnalyzeTargetsCount.Should().Be(0);
        }

        [Fact]
        public void Run_DispatchesToSynchronousValidateContext()
        {
            var command = new DispatchRecordingCommand();

            TestAnalysisContext context = null;
            command.Run(CreateOptions(), ref context);

            command.SynchronousValidateContextCount.Should().Be(1);
            command.AsynchronousValidateContextCount.Should().Be(0);
        }

        [Fact]
        public async Task RunAsync_DispatchesToAsynchronousAnalyzeTargets()
        {
            var command = new DispatchRecordingCommand();

            await command.RunAsync(CreateOptions(), globalContext: null);

            command.AsynchronousAnalyzeTargetsCount.Should().Be(1);
            command.SynchronousAnalyzeTargetsCount.Should().Be(0);
        }

        [Fact]
        public async Task RunAsync_DispatchesToAsynchronousValidateContext()
        {
            var command = new DispatchRecordingCommand();

            await command.RunAsync(CreateOptions(), globalContext: null);

            command.AsynchronousValidateContextCount.Should().Be(1);
            command.SynchronousValidateContextCount.Should().Be(0);
        }

        [Fact]
        public async Task RunAsync_PostsLogFileWhenHealthCheckSucceeds()
        {
            string outputFilePath = Path.GetTempFileName();

            try
            {
                Mock<HttpClientWrapper> httpClient = CreateHttpClient(HttpStatusCode.Accepted, HttpStatusCode.OK);

                TestAnalyzeOptions options = CreateOptions(AnalyzeThisAssembly, outputFilePath);
                options.PostUri = "https://example.com";

                (int exitCode, TestAnalysisContext context) =
                    await CreateCommand(httpClient.Object).RunAsync(options, globalContext: null);

                exitCode.Should().Be(SUCCESS);
                context.RuntimeErrors.Should().Be(RuntimeConditions.None);
            }
            finally
            {
                File.Delete(outputFilePath);
            }
        }

        [Fact]
        public async Task RunAsync_FailsWhenPostUriHealthCheckIsRejected()
        {
            Mock<HttpClientWrapper> httpClient = CreateHttpClient(HttpStatusCode.NotFound, HttpStatusCode.OK);

            TestAnalyzeOptions options = CreateOptions();
            options.PostUri = "https://example.com";

            (int exitCode, TestAnalysisContext context) =
                await CreateCommand(httpClient.Object).RunAsync(options, globalContext: null);

            exitCode.Should().Be(FAILURE);
            context.RuntimeErrors.HasFlag(RuntimeConditions.ExceptionPostingLogFile).Should().BeTrue();
            context.PostUri.Should().BeNullOrEmpty();
        }

        private static async Task<(RunOutcome Synchronous, RunOutcome Asynchronous)> AnalyzeBothWaysAsync(string targetSpecifier)
        {
            string synchronousOutputFilePath = Path.GetTempFileName();
            string asynchronousOutputFilePath = Path.GetTempFileName();

            try
            {
                TestAnalysisContext synchronousContext = null;
                int synchronousExitCode = CreateCommand()
                    .Run(CreateOptions(targetSpecifier, synchronousOutputFilePath), ref synchronousContext);

                (int asynchronousExitCode, TestAnalysisContext asynchronousContext) = await CreateCommand()
                    .RunAsync(CreateOptions(targetSpecifier, asynchronousOutputFilePath), globalContext: null);

                synchronousContext.Disposed.Should().BeTrue();
                asynchronousContext.Disposed.Should().BeTrue();

                return (new RunOutcome(synchronousExitCode, synchronousContext.RuntimeErrors, ReadRun(synchronousOutputFilePath)),
                        new RunOutcome(asynchronousExitCode, asynchronousContext.RuntimeErrors, ReadRun(asynchronousOutputFilePath)));
            }
            finally
            {
                File.Delete(synchronousOutputFilePath);
                File.Delete(asynchronousOutputFilePath);
            }
        }

        private static async Task WaitForAsync(Task task)
        {
            Task completed = await Task.WhenAny(task, Task.Delay(PipelineTimeout));
            completed.Should().BeSameAs(task, "the analysis pipeline should have reached the awaited stage");
            await task;
        }

        private static Run ReadRun(string outputFilePath)
        {
            if (!File.Exists(outputFilePath)) { return null; }

            string text = File.ReadAllText(outputFilePath);
            if (string.IsNullOrWhiteSpace(text)) { return null; }

            return JsonConvert.DeserializeObject<SarifLog>(text)?.Runs?.FirstOrDefault();
        }

        private static Mock<HttpClientWrapper> CreateHttpClient(HttpStatusCode healthCheckStatus, HttpStatusCode postStatus)
        {
            var httpClient = new Mock<HttpClientWrapper>();

            httpClient.Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                      .ReturnsAsync((string uriString, HttpContent content) =>
                          new HttpResponseMessage(
                              new Uri(uriString).Query.Contains("healthcheck=true")
                                  ? healthCheckStatus
                                  : postStatus));

            return httpClient;
        }

        private static TestMultithreadedAnalyzeCommand CreateCommand(HttpClientWrapper httpClientWrapper = null)
        {
            return new TestMultithreadedAnalyzeCommand(FileSystem.Instance, httpClientWrapper)
            {
                DefaultPluginAssemblies = TestPluginAssemblies,
            };
        }

        private static TestAnalyzeOptions CreateOptions(string targetSpecifier = AnalyzeThisAssembly,
                                                        string outputFilePath = null)
        {
            return new TestAnalyzeOptions
            {
                Quiet = true,
                Recurse = false,
                OutputFilePath = outputFilePath,
                SarifOutputVersion = SarifVersion.Current,
                TestRuleBehaviors = TestRuleBehaviors.LogError,
                OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                ConfigurationFilePath = TestMultithreadedAnalyzeCommand.DefaultPolicyName,
                TargetFileSpecifiers = targetSpecifier == AnalyzeNothing
                    ? Array.Empty<string>()
                    : new[] { ThisTestAssemblyFilePath },
            };
        }

        // Sentinels selecting a target set. A const is required to serve as an optional
        // parameter default, so the assembly path itself is resolved by CreateOptions.
        private const string AnalyzeThisAssembly = nameof(AnalyzeThisAssembly);
        private const string AnalyzeNothing = nameof(AnalyzeNothing);

        private static string ThisTestAssemblyFilePath
            => typeof(MultithreadedAnalyzeCommandBaseAsyncTests).Assembly.Location;

        private static Assembly[] TestPluginAssemblies
            => new[] { typeof(MultithreadedAnalyzeCommandBaseAsyncTests).Assembly };

        private sealed class RunOutcome
        {
            internal RunOutcome(int exitCode, RuntimeConditions runtimeErrors, Run run)
            {
                ExitCode = exitCode;
                RuntimeErrors = runtimeErrors;
                Run = run;
            }

            internal int ExitCode { get; }

            internal RuntimeConditions RuntimeErrors { get; }

            internal Run Run { get; }
        }

        /// <summary>
        /// Suspends analysis until <see cref="OpenGate"/> is called, letting a test observe the
        /// pipeline while a scan is in flight.
        /// </summary>
        private sealed class GatedAnalyzeCommand : TestMultithreadedAnalyzeCommand
        {
            private readonly TaskCompletionSource<bool> gate =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            internal GatedAnalyzeCommand()
                : base(Sarif.FileSystem.Instance)
            {
                DefaultPluginAssemblies = TestPluginAssemblies;
            }

            internal TaskCompletionSource<bool> AnalysisStarted { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            internal void OpenGate() => this.gate.TrySetResult(true);

            protected override async Task AnalyzeTargetsAsync(TestAnalysisContext context,
                                                              IEnumerable<Skimmer<TestAnalysisContext>> skimmers)
            {
                AnalysisStarted.TrySetResult(true);
                await this.gate.Task;
                await base.AnalyzeTargetsAsync(context, skimmers);
            }
        }

        /// <summary>
        /// Records which member of each synchronous / asynchronous virtual pair the engine
        /// dispatched to, so that the async pipeline cannot silently orphan a subclass that
        /// overrides the synchronous members.
        /// </summary>
        private sealed class DispatchRecordingCommand : TestMultithreadedAnalyzeCommand
        {
            private int synchronousAnalyzeTargetsCount;
            private int asynchronousAnalyzeTargetsCount;
            private int synchronousValidateContextCount;
            private int asynchronousValidateContextCount;

            internal DispatchRecordingCommand()
                : base(Sarif.FileSystem.Instance)
            {
                DefaultPluginAssemblies = TestPluginAssemblies;
            }

            internal int SynchronousAnalyzeTargetsCount => Volatile.Read(ref this.synchronousAnalyzeTargetsCount);

            internal int AsynchronousAnalyzeTargetsCount => Volatile.Read(ref this.asynchronousAnalyzeTargetsCount);

            internal int SynchronousValidateContextCount => Volatile.Read(ref this.synchronousValidateContextCount);

            internal int AsynchronousValidateContextCount => Volatile.Read(ref this.asynchronousValidateContextCount);

            public override TestAnalysisContext ValidateContext(TestAnalysisContext globalContext)
            {
                Interlocked.Increment(ref this.synchronousValidateContextCount);
                return base.ValidateContext(globalContext);
            }

            public override Task<TestAnalysisContext> ValidateContextAsync(TestAnalysisContext globalContext)
            {
                Interlocked.Increment(ref this.asynchronousValidateContextCount);
                return base.ValidateContextAsync(globalContext);
            }

            protected override void AnalyzeTargets(TestAnalysisContext context,
                                                   IEnumerable<Skimmer<TestAnalysisContext>> skimmers)
            {
                Interlocked.Increment(ref this.synchronousAnalyzeTargetsCount);
                base.AnalyzeTargets(context, skimmers);
            }

            protected override Task AnalyzeTargetsAsync(TestAnalysisContext context,
                                                        IEnumerable<Skimmer<TestAnalysisContext>> skimmers)
            {
                Interlocked.Increment(ref this.asynchronousAnalyzeTargetsCount);
                return base.AnalyzeTargetsAsync(context, skimmers);
            }
        }

        /// <summary>
        /// Never executes what it is handed. Any await that captures this context strands its
        /// continuation, which surfaces as a deadlock rather than as a silent regression.
        /// </summary>
        private sealed class RecordingSynchronizationContext : SynchronizationContext
        {
            private int postCount;

            internal int PostCount => Volatile.Read(ref this.postCount);

            public override void Post(SendOrPostCallback d, object state)
                => Interlocked.Increment(ref this.postCount);

            public override void Send(SendOrPostCallback d, object state)
                => Interlocked.Increment(ref this.postCount);
        }
    }
}
