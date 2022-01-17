// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
            random = RandomSarifLogGenerator.GenerateRandomAndLog(outputHelper);
        }

        [Fact]
        public void SortingVisitor_ShuffleTest()
        {
            bool areEqual;
            // create a test sarif log
            SarifLog originalLog = CreateTestSarifLog(this.random);

            SarifLog shuffledLog1 = originalLog.DeepClone();
            SarifLog shuffledLog2 = originalLog.DeepClone();

            // original log and cloned log should be same
            areEqual = SarifLogEqualityComparer.Instance.Equals(originalLog, shuffledLog1);
            areEqual.Should().BeTrue();

            areEqual = SarifLogEqualityComparer.Instance.Equals(originalLog, shuffledLog2);
            areEqual.Should().BeTrue();

            areEqual = SarifLogEqualityComparer.Instance.Equals(shuffledLog1, shuffledLog2);
            areEqual.Should().BeTrue();

            // shuffle sarif log
            ShuffleSarifLog(shuffledLog1, this.random);
            ShuffleSarifLog(shuffledLog2, this.random);

            // shuffled logs should be not same as each other and original log.
            areEqual = SarifLogEqualityComparer.Instance.Equals(originalLog, shuffledLog1);
            areEqual.Should().BeFalse();

            areEqual = SarifLogEqualityComparer.Instance.Equals(originalLog, shuffledLog2);
            areEqual.Should().BeFalse();

            areEqual = SarifLogEqualityComparer.Instance.Equals(shuffledLog1, shuffledLog2);
            areEqual.Should().BeFalse();

            // sort shuffled logs using visitor
            SarifLog sortedLog1 = new SortingVisitor().VisitSarifLog(shuffledLog1);
            SarifLog sortedLog2 = new SortingVisitor().VisitSarifLog(shuffledLog2);

            // verify sorted logs should be same (deterministic)
            // sorted logs may not be same with original log due to random values
            areEqual = SarifLogEqualityComparer.Instance.Equals(sortedLog1, sortedLog2);
            areEqual.Should().BeTrue();

            // make sure result's ruleIndex points to right rule in sorted log
            IList<ReportingDescriptor> rules = sortedLog1.Runs.First().Tool.Driver.Rules;
            foreach (Result result in sortedLog1.Runs.First().Results)
            {
                if (result.RuleIndex != -1)
                {
                    int ruleIndex = rules.IndexOf(rules.First(r => r.Id.Equals(result.RuleId)));
                    result.RuleIndex.Should().Be(ruleIndex);
                }
            }

            // make sure artifactLocation index points to right artifacts
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
            // arrange
            // create a log with all results have same value.
            SarifLog sarifLog = new SarifLog
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

            // setup results
            IList<Result> results = sarifLog.Runs[0].Results;
            results[0].Locations = new[] { new Location { Message = new Message { Text = "test location" } } };
            results[1].Locations = null;
            results[2].Locations = new List<Location>();

            // act
            SarifLog sortedLog = new SortingVisitor().VisitSarifLog(sarifLog);

            // assert
            // if collection elements all values are same expect a child list
            // the order should depend on list values.
            // expected order: null < empty < collection has element
            results = sortedLog.Runs[0].Results;
            results[0].Locations.Should().BeNull();
            results[1].Locations.Should().BeEmpty();
            results[2].Locations.Should().NotBeEmpty();
        }

        private static SarifLog CreateTestSarifLog(Random random)
        {
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, runCount: 1, resultCount: random.Next(100));

            CreateCodeFlows(sarifLog.Runs?.First()?.Results, sarifLog.Runs?.First()?.Artifacts, random);

            return sarifLog;
        }

        private static void CreateCodeFlows(IList<Result> results, IList<Artifact> artifacts, Random random)
        {
            if (results?.Any() != true || artifacts?.Any() != true)
            {
                return;
            }

            foreach (Result result in results)
            {
                result.CodeFlows = new[]
                {
                    new CodeFlow
                    {
                        ThreadFlows = new []
                        {
                            new ThreadFlow
                            {
                                Locations = GenerateRandomThreadFlowLocations(
                                    count: random.Next(10), artifacts, random)
                            }
                        }
                    }
                };
            }
        }

        private static IList<ThreadFlowLocation> GenerateRandomThreadFlowLocations(int count, IList<Artifact> artifacts, Random random)
        {
            var locations = new List<ThreadFlowLocation>();

            for (int i = 0; i < count; i++)
            {
                locations.Add(new ThreadFlowLocation
                {
                    Importance = RandomEnumValue<ThreadFlowLocationImportance>(random),
                    Location = new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Index = random.Next(artifacts.Count),
                            },
                            Region = new Region
                            {
                                StartLine = random.Next(500),
                                StartColumn = random.Next(100),
                            },
                        },
                    },
                });
            }
            return locations;
        }

        private static T RandomEnumValue<T>(Random random) where T : Enum
        {
            Array enums = Enum.GetValues(typeof(T));
            return (T)enums.GetValue(random.Next(enums.Length));
        }

        private static void ShuffleSarifLog(SarifLog log, Random random)
        {
            Run run = log.Runs.First();
            IList<ReportingDescriptor> rules = run?.Tool?.Driver?.Rules;

            if (rules != null)
            {
                IDictionary<string, int> ruleIndexMapping = new Dictionary<string, int>();

                // shuffle rules
                rules = rules.Shuffle(random);
                log.Runs.First().Tool.Driver.Rules = rules;

                // store new rules indexes 
                for (int i = 0; i < rules.Count; i++)
                {
                    ruleIndexMapping.Add(rules[i].Id, i);
                }

                // update results rule index
                IList<Result> results = log?.Runs?.First()?.Results;
                foreach (Result result in results)
                {
                    if (result.RuleIndex != -1
                        && ruleIndexMapping.TryGetValue(result.RuleId, out int newIndex))
                    {
                        result.RuleIndex = newIndex;

                    }
                }

                IList<Artifact> artifacts = run?.Artifacts;
                if (artifacts != null)
                {
                    IDictionary<int, int> artifactIndexMapping = new Dictionary<int, int>();

                    // store old artifacts indexes
                    IDictionary<Artifact, int> oldMapping = new Dictionary<Artifact, int>();
                    for (int i = 0; i < artifacts.Count; i++)
                    {
                        oldMapping.Add(artifacts[i], i);
                    }

                    // shuffle artifacts
                    artifacts = artifacts.Shuffle(random);
                    log.Runs.First().Artifacts = artifacts;

                    // store new artifacts indexes 
                    for (int i = 0; i < artifacts.Count; i++)
                    {
                        if (oldMapping.TryGetValue(artifacts[i], out int oldIndex))
                        {
                            artifactIndexMapping.Add(oldIndex, i);
                        }
                    }

                    // update all artifacts' index if its set.
                    List<ArtifactLocation> locToUpdate = new List<ArtifactLocation>();
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

                    foreach (ArtifactLocation artifactLocation in locToUpdate)
                    {
                        if (artifactLocation.Index != -1
                            && artifactIndexMapping.TryGetValue(artifactLocation.Index, out int newIndex))
                        {
                            artifactLocation.Index = newIndex;

                        }
                    }
                }

                // shuffle results
                log.Runs.First().Results = results.Shuffle();

                // shuffle codeflow locations
                foreach (Result result in log.Runs.First().Results)
                {
                    result.CodeFlows = result.CodeFlows.Shuffle();
                    foreach (CodeFlow codeFlow in result.CodeFlows)
                    {
                        codeFlow.ThreadFlows = codeFlow.ThreadFlows.Shuffle();
                        foreach (ThreadFlow threadFlow in codeFlow.ThreadFlows)
                        {
                            threadFlow.Locations = threadFlow.Locations.Shuffle();
                        }
                    }
                }
            }
        }
    }
}
