// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Visitors
{
    public class SortingVisitorTests : FileDiffingUnitTests
    {
        private readonly Random random;
        private readonly ITestOutputHelper _outputHelper;

        public SortingVisitorTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            this._outputHelper = outputHelper;
            this.random = RandomSarifLogGenerator.GenerateRandomAndLog(outputHelper);
        }

        [Fact]
        public void SortingVisitor_ShuffleTest()
        {
            bool areEqual;

            SarifLog originalLog = CreateTestSarifLog(this.random);
            SarifLog shuffledLog1 = ShuffleSarifLog(originalLog, this.random);
            SarifLog shuffledLog2 = ShuffleSarifLog(originalLog, this.random);

            areEqual = SarifLogEqualityComparer.Instance.Equals(originalLog, shuffledLog1);
            areEqual.Should().BeFalse();

            areEqual = SarifLogEqualityComparer.Instance.Equals(originalLog, shuffledLog2);
            areEqual.Should().BeFalse();

            areEqual = SarifLogEqualityComparer.Instance.Equals(shuffledLog1, shuffledLog2);
            areEqual.Should().BeFalse();

            SarifLog sortedLog1 = new SortingVisitor().VisitSarifLog(shuffledLog1);
            SarifLog sortedLog2 = new SortingVisitor().VisitSarifLog(shuffledLog2);

            areEqual = this.VerifySarifLogAreSame(sortedLog1, sortedLog2);
            areEqual.Should().BeTrue();

            areEqual = SarifLogEqualityComparer.Instance.Equals(sortedLog1, sortedLog2);
            areEqual.Should().BeTrue();

            foreach (Run run in sortedLog1.Runs)
            {
                IList<ReportingDescriptor> rules = run.Tool.Driver.Rules;
                IList<Artifact> artifacts = run.Artifacts;

                foreach (Result result in run.Results)
                {
                    if (result.RuleIndex != -1)
                    {
                        int ruleIndex = rules.IndexOf(rules.First(r => r.Id.Equals(result.RuleId)));
                        result.RuleIndex.Should().Be(ruleIndex);
                    }

                    ArtifactLocation artifactLoc = result?.Locations?.First()?.PhysicalLocation?.ArtifactLocation;

                    if (artifactLoc != null && artifactLoc.Index != -1)
                    {
                        artifactLoc.Uri.Should().Be(artifacts[artifactLoc.Index].Location.Uri);
                    }
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

            while (SarifLogEqualityComparer.Instance.Equals(logToBeShuffled, originalLog))
            {
                foreach (Run run in logToBeShuffled?.Runs)
                {
                    IList<ReportingDescriptor> rules = run?.Tool?.Driver?.Rules;
                    IList<Result> results = run?.Results;
                    IList<Artifact> artifacts = run?.Artifacts;

                    if (rules != null)
                    {
                        var ruleIndexMapping = new Dictionary<string, int>();
                        rules = rules.Shuffle(random);
                        run.Tool.Driver.Rules = rules;

                        for (int i = 0; i < rules.Count; i++)
                        {
                            ruleIndexMapping[rules[i].Id] = i;
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
                        var artifactIndexMapping = new Dictionary<int, int>();
                        var oldMapping = new Dictionary<Artifact, int>();

                        for (int i = 0; i < artifacts.Count; i++)
                        {
                            oldMapping[artifacts[i]] = i;
                        }

                        artifacts = artifacts.Shuffle(random);
                        run.Artifacts = artifacts;

                        for (int i = 0; i < artifacts.Count; i++)
                        {
                            if (oldMapping.TryGetValue(artifacts[i], out int oldIndex))
                            {
                                artifactIndexMapping[oldIndex] = i;
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

            return logToBeShuffled;
        }

        private bool VerifySarifLogAreSame(SarifLog first, SarifLog second)
        {
            string firstLogText = ReadSarifLogAsString(first);
            string secondLogText = ReadSarifLogAsString(second);

            bool areEqual = AreEquivalent<SarifLog>(firstLogText, secondLogText, out SarifLog _);

            if (!areEqual)
            {
                // NewGuid() is used here simply to ensure we create a unique file name.
                string firstLogFile = GetOutputFilePath(directory: null, $"{Guid.NewGuid()}.sarif");
                string secondLogFile = GetOutputFilePath(directory: null, $"{Guid.NewGuid()}.sarif");

                File.WriteAllText(firstLogFile, firstLogText);
                File.WriteAllText(secondLogFile, secondLogText);

                var sb = new StringBuilder();
                sb.AppendLine("The sorted Sarif logs did not match.");
                sb.AppendLine("To compare all difference for this test suite:");
                sb.AppendLine(FileDiffingUnitTests.GenerateDiffCommand(TypeUnderTest, firstLogFile, secondLogFile));
                this._outputHelper.WriteLine(sb.ToString());
            }

            return areEqual;
        }

        private static string ReadSarifLogAsString(SarifLog sarifLog)
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(sarifLog, settings);
        }
    }
}
