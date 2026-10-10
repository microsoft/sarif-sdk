// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class ToolSerializationRaceTests
    {
        [Fact]
        public Task Run_WaitsForLateDriverRuleLoggingBeforeDisposal()
            => VerifyTimeoutWaitsForAnalysisAsync(runAsynchronously: false, useExtension: false);

        [Fact]
        public Task RunAsync_WaitsForLateDriverRuleLoggingBeforeDisposal()
            => VerifyTimeoutWaitsForAnalysisAsync(runAsynchronously: true, useExtension: false);

        [Fact]
        public Task Run_WaitsForLateExtensionRuleLoggingBeforeDisposal()
            => VerifyTimeoutWaitsForAnalysisAsync(runAsynchronously: false, useExtension: true);

        [Fact]
        public Task RunAsync_WaitsForLateExtensionRuleLoggingBeforeDisposal()
            => VerifyTimeoutWaitsForAnalysisAsync(runAsynchronously: true, useExtension: true);

        private async Task VerifyTimeoutWaitsForAnalysisAsync(bool runAsynchronously, bool useExtension)
        {
            using var analysisStarted = new ManualResetEventSlim();
            using var releaseAnalysis = new ManualResetEventSlim();
            using var serializationStarted = new ManualResetEventSlim();
            using var writer = new GatedWriter(serializationStarted);
            var command = new GatedCommand(writer, analysisStarted, releaseAnalysis, useExtension);
            var context = new TestAnalysisContext();
            var options = new TestAnalyzeOptions
            {
                Quiet = true,
                TimeoutInSeconds = 1,
                ConfigurationFilePath = TestMultithreadedAnalyzeCommand.DefaultPolicyName,
                TargetFileSpecifiers = new[] { typeof(ToolSerializationRaceTests).Assembly.Location },
            };

            Task<(int ExitCode, TestAnalysisContext Context)> runTask = runAsynchronously
                ? command.RunAsync(options, context)
                : Task.Run(() => RunSynchronously(command, options, context));

            try
            {
                Assert.True(await Task.Run(() => analysisStarted.Wait(TimeSpan.FromSeconds(15))));
                Assert.True(await Task.Run(() => SpinWait.SpinUntil(
                    () => context.RuntimeErrors.HasFlag(RuntimeConditions.AnalysisTimedOut),
                    TimeSpan.FromSeconds(15))));
                Assert.False(runTask.IsCompleted);
                Assert.False(serializationStarted.IsSet);

                releaseAnalysis.Set();

                (int exitCode, TestAnalysisContext usedContext) =
                    await runTask.WaitAsync(TimeSpan.FromSeconds(15));

                Assert.Same(context, usedContext);
                Assert.Equal(CommandBase.FAILURE, exitCode);
                Assert.True(context.RuntimeErrors.HasFlag(RuntimeConditions.AnalysisTimedOut));
                ToolComponent component = useExtension ? command.Tool.Extensions[0] : command.Tool.Driver;
                Assert.Contains(component.Rules, rule => rule.Id == "late-rule");
                Assert.True(serializationStarted.IsSet);
            }
            finally
            {
                releaseAnalysis.Set();
                await runTask.WaitAsync(TimeSpan.FromSeconds(15));
            }
        }

        private static (int ExitCode, TestAnalysisContext Context) RunSynchronously(
            TestMultithreadedAnalyzeCommand command,
            TestAnalyzeOptions options,
            TestAnalysisContext context)
        {
            int exitCode = command.Run(options, ref context);
            return (exitCode, context);
        }

        private sealed class GatedCommand : TestMultithreadedAnalyzeCommand
        {
            private readonly TextWriter writer;
            private readonly ManualResetEventSlim analysisStarted;
            private readonly ManualResetEventSlim releaseAnalysis;
            private readonly bool useExtension;

            internal GatedCommand(TextWriter writer,
                                  ManualResetEventSlim analysisStarted,
                                  ManualResetEventSlim releaseAnalysis,
                                  bool useExtension)
            {
                this.writer = writer;
                this.analysisStarted = analysisStarted;
                this.releaseAnalysis = releaseAnalysis;
                this.useExtension = useExtension;
                DefaultPluginAssemblies = new[] { typeof(ToolSerializationRaceTests).Assembly };
                var component = new ToolComponent
                {
                    Name = "RaceRepro",
                    Rules = new List<ReportingDescriptor>
                    {
                        new ReportingDescriptor { Id = "seed-rule" },
                    },
                };
                Tool = new Tool
                {
                    Driver = useExtension ? new ToolComponent { Name = "RaceRepro" } : component,
                    Extensions = useExtension ? new List<ToolComponent> { component } : null,
                };
            }

            public override void InitializeOutputs(TestAnalysisContext context)
            {
                var logger = new SarifLogger(this.writer, run: new Run { Tool = Tool },
                                             closeWriterOnDispose: false);
                ((AggregatingLogger)context.Logger).Loggers.Add(logger);
            }

            protected override void AnalyzeTargets(TestAnalysisContext context,
                                                   IEnumerable<Skimmer<TestAnalysisContext>> skimmers)
            {
                this.analysisStarted.Set();
                this.releaseAnalysis.Wait();
                LogLateRule(context);
            }

            protected override async Task AnalyzeTargetsAsync(TestAnalysisContext context,
                                                              IEnumerable<Skimmer<TestAnalysisContext>> skimmers)
            {
                this.analysisStarted.Set();
                await Task.Run(() => this.releaseAnalysis.Wait()).ConfigureAwait(false);
                LogLateRule(context);
            }

            private void LogLateRule(TestAnalysisContext context)
            {
                var rule = new ReportingDescriptor { Id = "late-rule" };
                if (this.useExtension)
                {
                    context.Logger.Log(rule,
                        new Result
                        {
                            RuleId = rule.Id,
                            Level = FailureLevel.Warning,
                            Message = new Message { Text = "late result" },
                        },
                        extensionIndex: 0);
                }
                else
                {
                    context.Logger.LogToolNotification(
                        new Notification
                        {
                            Level = FailureLevel.Warning,
                            Message = new Message { Text = "late notification" },
                        },
                        rule);
                }
            }
        }

        private sealed class GatedWriter : StringWriter
        {
            private readonly ManualResetEventSlim serializationStarted;

            internal GatedWriter(ManualResetEventSlim serializationStarted)
            {
                this.serializationStarted = serializationStarted;
            }

            public override void Write(char value)
            {
                base.Write(value);
                DetectToolSerialization();
            }

            public override void Write(string value)
            {
                base.Write(value);
                DetectToolSerialization();
            }

            public override void Write(char[] buffer, int index, int count)
            {
                base.Write(buffer, index, count);
                DetectToolSerialization();
            }

            private void DetectToolSerialization()
            {
                if (ToString().Contains("\"seed-rule\""))
                {
                    this.serializationStarted.Set();
                }
            }
        }
    }
}
