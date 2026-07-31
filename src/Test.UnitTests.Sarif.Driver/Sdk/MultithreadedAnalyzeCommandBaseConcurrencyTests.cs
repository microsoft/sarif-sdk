// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class MultithreadedAnalyzeCommandBaseConcurrencyTests
    {
        [Fact]
        public void Run_DoesNotLoseConcurrentRuntimeErrorMerges()
        {
            var globalContext = new MergeProbingContext();
            var command = new MergeProbingCommand(globalContext);

            TestAnalysisContext context = globalContext;
            command.Run(CreateOptions(), ref context);

            command.TargetsTagged.Should().BeGreaterThan(1,
                "a single target cannot produce the concurrent merges this test probes");
            globalContext.ClobberedMerge.Should().BeFalse(
                "every merge into RuntimeErrors must be serialized, or a runtime condition is silently dropped");
        }

        [Fact]
        public async Task RunAsync_DoesNotLoseConcurrentRuntimeErrorMerges()
        {
            var globalContext = new MergeProbingContext();
            var command = new MergeProbingCommand(globalContext);

            await command.RunAsync(CreateOptions(), globalContext);

            command.TargetsTagged.Should().BeGreaterThan(1,
                "a single target cannot produce the concurrent merges this test probes");
            globalContext.ClobberedMerge.Should().BeFalse(
                "every merge into RuntimeErrors must be serialized, or a runtime condition is silently dropped");
        }

        private static TestAnalyzeOptions CreateOptions()
        {
            return new TestAnalyzeOptions
            {
                Quiet = true,
                Recurse = false,
                Threads = 8,
                SarifOutputVersion = SarifVersion.Current,
                TestRuleBehaviors = TestRuleBehaviors.LogError,
                ConfigurationFilePath = TestMultithreadedAnalyzeCommand.DefaultPolicyName,
                TargetFileSpecifiers = new[]
                {
                    Path.Combine(Path.GetDirectoryName(ThisTestAssemblyFilePath), "*.dll"),
                },
            };
        }

        private static string ThisTestAssemblyFilePath
            => typeof(MultithreadedAnalyzeCommandBaseConcurrencyTests).Assembly.Location;

        private static Assembly[] TestPluginAssemblies
            => new[] { typeof(MultithreadedAnalyzeCommandBaseConcurrencyTests).Assembly };

        /// <summary>
        /// Tags every scan target with a distinct non-fatal runtime condition, so that a merge
        /// which overwrites another is observable as a dropped flag rather than an idempotent
        /// rewrite of the value the engine already held.
        /// </summary>
        private sealed class MergeProbingCommand : TestMultithreadedAnalyzeCommand
        {
            private const int FirstProbeBit = 42;
            private const int ProbeBitCount = 22;

            private readonly MergeProbingContext globalContext;
            private int targetsTagged;

            internal MergeProbingCommand(MergeProbingContext globalContext)
                : base(Sarif.FileSystem.Instance)
            {
                this.globalContext = globalContext;
                DefaultPluginAssemblies = TestPluginAssemblies;
            }

            internal int TargetsTagged => this.targetsTagged;

            protected override TestAnalysisContext DetermineApplicabilityAndAnalyze(
                TestAnalysisContext context,
                IEnumerable<Skimmer<TestAnalysisContext>> skimmers,
                ISet<string> disabledSkimmers)
            {
                // Confine the probe's stall budget to the scan phase, where merges are
                // concurrent, rather than spending it on single-threaded configuration reads.
                this.globalContext.BeginProbing();

                TestAnalysisContext analyzed = base.DetermineApplicabilityAndAnalyze(context, skimmers, disabledSkimmers);

                int index = Interlocked.Increment(ref this.targetsTagged) - 1;
                analyzed.RuntimeErrors |= (RuntimeConditions)(1L << (FirstProbeBit + (index % ProbeBitCount)));

                return analyzed;
            }
        }
    }
}
