// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Core
{
    public class SarifLogTests
    {
        [Fact]
        public void SarifLog_DoesNotSerializeNonNullEmptyCollections()
        {
            var run = new Run
            {
                Graphs = new Graph[] { },
                Artifacts = new Artifact[] { },
                Invocations = new Invocation[] { },
                LogicalLocations = new LogicalLocation[] { }
            };

            run.Graphs.Should().NotBeNull();
            run.Artifacts.Should().NotBeNull();
            run.Invocations.Should().NotBeNull();
            run.LogicalLocations.Should().NotBeNull();

            run = SerializeAndDeserialize(run);

            // Certain non-null but entirely empty collections should not
            // be persisted during serialization. As a result, these properties
            // should be null after round-tripping, reflecting the actual
            // (i.e., entirely absent) representation on disk when saved.

            run.Graphs.Should().BeNull();
            run.Artifacts.Should().BeNull();
            run.Invocations.Should().BeNull();
            run.LogicalLocations.Should().BeNull();

            // If arrays are non-empty but only contain object instances
            // that consist of nothing but default values, these also
            // should not be persisted to disk
            run.Graphs = new Graph[] { new Graph() };
            run.Artifacts = new Artifact[] { new Artifact() };
            run.LogicalLocations = new LogicalLocation[] { new LogicalLocation() };

            // Invocations are special, they have a required property,
            // ExecutionSuccessful. This means even an entirely default instance
            // should be retained when serialized.
            run.Invocations = new Invocation[] { new Invocation() };

            run = SerializeAndDeserialize(run);

            run.Graphs.Should().BeNull();
            run.Artifacts.Should().BeNull();
            run.LogicalLocations.Should().BeNull();

            run.Invocations.Should().NotBeNull();
        }

        [Fact]
        public void SarifLog_ApplyPoliciesShouldNotThrowWhenRunsDoesNotExist()
        {
            var sarifLog = new SarifLog();
            Action action = () => sarifLog.ApplyPolicies();

            action.Should().NotThrow();
        }

        [Fact]
        public void SarifLog_SplitPerRun()
        {
            var random = new Random();
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            sarifLog.Split(SplittingStrategy.PerRun).Should().HaveCount(1);

            sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 3);
            sarifLog.Split(SplittingStrategy.PerRun).Should().HaveCount(3);
        }

        [Fact]
        public void SarifLog_SplitPerResult()
        {
            var random = new Random();
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            IList<SarifLog> logs = sarifLog.Split(SplittingStrategy.PerResult).ToList();
            logs.Count.Should().Be(5);
            foreach (SarifLog log in logs)
            {
                // optimized partitioned log should only include rules referenced by its results
                log.Runs.Count.Should().Be(1);
                int ruleCount = log.Runs[0].Results.Select(r => r.RuleId).Distinct().Count();
                ruleCount.Should().Be(log.Runs[0].Tool.Driver.Rules.Count);
            }
        }

        private Run SerializeAndDeserialize(Run run)
        {
            return JsonConvert.DeserializeObject<Run>(JsonConvert.SerializeObject(run));
        }
    }
}
