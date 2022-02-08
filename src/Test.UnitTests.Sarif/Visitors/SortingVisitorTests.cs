// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Visitors
{
    public class SortingVisitorTests
    {
        private readonly Random random;

        public SortingVisitorTests(ITestOutputHelper outputHelper)
        {
            this.random = RandomSarifLogGenerator.GenerateRandomAndLog(outputHelper);
        }

        [Fact]
        public void SortingVisitor_ShuffleTest()
        {
            bool areEqual;

            SarifLog originalLog = CreateTestSarifLog(this.random);
            SarifLog shuffledLog1 = ShuffleSarifLog(originalLog, this.random);
            SarifLog shuffledLog2 = ShuffleSarifLog(originalLog, this.random);

            // Shuffled logs should be not same as each other and original log.
            areEqual = SarifLogEqualityComparer.Instance.Equals(originalLog, shuffledLog1);
            areEqual.Should().BeFalse();

            areEqual = SarifLogEqualityComparer.Instance.Equals(originalLog, shuffledLog2);
            areEqual.Should().BeFalse();

            areEqual = SarifLogEqualityComparer.Instance.Equals(shuffledLog1, shuffledLog2);
            areEqual.Should().BeFalse();

            SarifLog sortedLog1 = new SortingVisitor().VisitSarifLog(shuffledLog1);
            SarifLog sortedLog2 = new SortingVisitor().VisitSarifLog(shuffledLog2);

            areEqual = SarifLogEqualityComparer.Instance.Equals(sortedLog1, sortedLog2);
            areEqual.Should().BeTrue();

            // Make sure result's ruleIndex points to right rule in sorted log.
            IList<ReportingDescriptor> rules = sortedLog1.Runs.First().Tool.Driver.Rules;
            foreach (Result result in sortedLog1.Runs.First().Results.Where(r => r.RuleIndex != -1))
            {
                int ruleIndex = rules.IndexOf(rules.First(r => r.Id.Equals(result.RuleId)));
                result.RuleIndex.Should().Be(ruleIndex);
            }

            // Make sure artifactLocation index points to right artifacts.
            IList<Artifact> artifacts = sortedLog1.Runs.First().Artifacts;
            foreach (Result result in sortedLog1.Runs.First().Results)
            {
                ArtifactLocation artifactLoc = result?.Locations?.First()?.PhysicalLocation?.ArtifactLocation;
                if (artifactLoc != null && artifactLoc.Index != -1)
                {
                    artifactLoc.Uri.Should().Be(artifacts[artifactLoc.Index].Location.Uri);
                }
            }
        }

        [Fact]
        public void SortingVisitor_NullEmptyListTests()
        {
            // Create a log with all results have same values.
            var sarifLog = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Results = new[]
                        {
                            new Result
                            {
                                RuleId = "TESTRULE001"
                            },
                            new Result
                            {
                                RuleId = "TESTRULE001"
                            },
                            new Result
                            {
                                RuleId = "TESTRULE001"
                            },
                        },
                    },
                },
            };

            IList<Result> results = sarifLog.Runs[0].Results;
            results[0].Locations = new[] { new Location { Message = new Message { Text = "test location" } } };
            results[1].Locations = null;
            results[2].Locations = new List<Location>();

            SarifLog sortedLog = new SortingVisitor().VisitSarifLog(sarifLog);

            // If sorting a collection with element has a list type property
            // the order should depend on list values.
            // Expected order: null < empty < collection has element.
            results = sortedLog.Runs[0].Results;
            results[0].Locations.Should().BeNull();
            results[1].Locations.Should().BeEmpty();
            results[2].Locations.Should().NotBeEmpty();
        }

        private static SarifLog CreateTestSarifLog(Random random)
        {
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(
                randomGen: random,
                runCount: random.Next(1, 5),
                dataFields: RandomDataFields.CodeFlow | RandomDataFields.ThreadFlow | RandomDataFields.LogicalLocation);

            return sarifLog;
        }

        private static SarifLog ShuffleSarifLog(SarifLog originalLog, Random random)
        {
            SarifLog logToBeShuffled = originalLog.DeepClone();

            bool areEqual = SarifLogEqualityComparer.Instance.Equals(originalLog, logToBeShuffled);
            areEqual.Should().BeTrue();

            // Shuffle the log cloned from original log until
            // find the log is different than original log.
            do
            {
                foreach (Run run in logToBeShuffled?.Runs)
                {
                    IList<ReportingDescriptor> rules = run?.Tool?.Driver?.Rules;
                    IList<Result> results = run?.Results;
                    IList<Artifact> artifacts = run?.Artifacts;

                    if (rules != null)
                    {
                        IDictionary<string, int> ruleIndexMapping = new Dictionary<string, int>();

                        rules = rules.Shuffle(random);
                        run.Tool.Driver.Rules = rules;

                        for (int i = 0; i < rules.Count; i++)
                        {
                            ruleIndexMapping.Add(rules[i].Id, i);
                        }

                        foreach (Result result in results.Where(r => r.RuleIndex != -1))
                        {
                            if (ruleIndexMapping.TryGetValue(result.RuleId, out int newIndex))
                            {
                                result.RuleIndex = newIndex;
                            }
                        }
                    }

                    if (artifacts != null)
                    {
                        IDictionary<int, int> artifactIndexMapping = new Dictionary<int, int>();

                        IDictionary<Artifact, int> oldMapping = new Dictionary<Artifact, int>();
                        for (int i = 0; i < artifacts.Count; i++)
                        {
                            oldMapping.Add(artifacts[i], i);
                        }

                        artifacts = artifacts.Shuffle(random);
                        run.Artifacts = artifacts;

                        for (int i = 0; i < artifacts.Count; i++)
                        {
                            if (oldMapping.TryGetValue(artifacts[i], out int oldIndex))
                            {
                                artifactIndexMapping.Add(oldIndex, i);
                            }
                        }

                        var locToUpdate = new List<ArtifactLocation>();
                        locToUpdate.AddRange(
                            results
                            .SelectMany(r => r.Locations)
                            .Select(l => l.PhysicalLocation.ArtifactLocation));

                        locToUpdate.AddRange(
                            results
                            .SelectMany(r => r.CodeFlows)
                            .SelectMany(c => c.ThreadFlows)
                            .SelectMany(t => t.Locations)
                            .Select(l => l.Location.PhysicalLocation.ArtifactLocation));

                        foreach (ArtifactLocation artifactLocation in locToUpdate.Where(l => l.Index != -1))
                        {
                            if (artifactIndexMapping.TryGetValue(artifactLocation.Index, out int newIndex))
                            {
                                artifactLocation.Index = newIndex;
                            }
                        }
                    }


                    run.Results = results.Shuffle(random);

                    foreach (Result result in run.Results)
                    {
                        result.CodeFlows = result.CodeFlows.Shuffle(random);
                        foreach (CodeFlow codeFlow in result.CodeFlows)
                        {
                            codeFlow.ThreadFlows = codeFlow.ThreadFlows.Shuffle(random);
                            foreach (ThreadFlow threadFlow in codeFlow.ThreadFlows)
                            {
                                threadFlow.Locations = threadFlow.Locations.Shuffle(random);
                            }
                        }
                    }
                }
                logToBeShuffled.Runs = logToBeShuffled.Runs.Shuffle(random);
            }
            while (SarifLogEqualityComparer.Instance.Equals(logToBeShuffled, originalLog));

            return logToBeShuffled;
        }
    }
}
