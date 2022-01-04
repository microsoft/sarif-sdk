// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Microsoft.VisualStudio.Services.Common;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemExtensionsTests
    {
        private readonly string testSarifFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Test", "Data", "File{0}.sarif");

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemTitle_HandlesSingleResultWithRuleIdOnly()
        {
            var sb = new StringBuilder();
            foreach (Tuple<string, Result> tuple in ResultsWithVariousRuleExpressions)
            {
                Result result = tuple.Item2;
                SarifLog sarifLog = CreateLogWithEmptyRun();

                Run run = sarifLog.Runs[0];
                run.Results.Add(tuple.Item2);

                string title = sarifLog.Runs[0].CreateWorkItemTitle();
                string ruleId = result.ResolvedRuleId(run);

                if (!title.Contains(ToolName))
                {
                    sb.AppendLine($"[Test case '{tuple.Item1}']: Title did not contain generated tool name.");
                }

                if (!title.Contains(ruleId))
                {
                    sb.AppendLine($"[Test case '{tuple.Item1}']: Title did not contain expected rule id '{ruleId}'.");
                }

                FailureLevel level = result.Level;
                if (!title.Contains(level.ToString()))
                {
                    sb.AppendLine($"[Test case '{tuple.Item1}']: Title did not contain expected failure level '{level}'.");
                }

                string location = result.Locations?[0].PhysicalLocation?.ArtifactLocation?.Uri?.OriginalString;
                if (!string.IsNullOrEmpty(location) && !title.Contains(location))
                {
                    sb.AppendLine($"[Test case '{tuple.Item1}']: Title did not contain expected location '{location}'.");
                }
            }

            sb.Length.Should().Be(0, because: Environment.NewLine + sb.ToString());
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemTitle_LongTitleFromUrl()
        {
            int maxLength = 255;
            string ruleId = "TestRuleId";
            string expectedTemplate = "[aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:Warning]: TestRuleId: Test Rule (in al...)";
            string expected = $":Warning]: TestRuleId: Test Rule (in al" + new string('a', maxLength - expectedTemplate.Length) + "...)";

            Result result = new Result();
            ArtifactLocation artifactLocation = new ArtifactLocation(new Uri("al" + new string('a', 1024), UriKind.Relative), string.Empty, 0, new Message(), new Dictionary<string, SerializedPropertyInfo>());
            PhysicalLocation physicalLocation = new PhysicalLocation(new Address(), artifactLocation, new Region(), new Region(), new Dictionary<string, SerializedPropertyInfo>());
            Location location = new Location(0, physicalLocation, null, null, null, null, null);
            result.Locations = new List<Location>();
            result.Locations.Add(location);
            result.RuleId = ruleId;
            SarifLog sarifLog = CreateLogWithEmptyRun();

            Run run = sarifLog.Runs[0];
            run.Results.Add(result);

            string title = sarifLog.Runs[0].CreateWorkItemTitle();

            title.Should().EndWith(expected);
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemTitle_LongTitleFromLogicalLocation()
        {
            Result result = new Result();
            LogicalLocation logicaLocation = new LogicalLocation(null, 0, string.Empty, null, 0, null, null);
            Location location = new Location(0, null, new[] { logicaLocation }, null, null, null, null);
            result.Locations = new List<Location>();
            result.Locations.Add(location);
            result.RuleId = "TestRuleId";

            SarifLog sarifLog = CreateLogWithEmptyRun();
            Run run = sarifLog.Runs[0];
            run.Results.Add(result);

            // A logical location longer than 128 char is truncated with ellipses
            int maxLength = 255;
            string expectedTemplate = "[aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:Warning]: TestRuleId: Test Rule (in 'll...')";
            string expected = $":Warning]: TestRuleId: Test Rule (in 'll" + new string('b', maxLength - expectedTemplate.Length) + "...')";
            result.Locations[0].LogicalLocation.FullyQualifiedName = "ll" + new string('b', 1024);
            string title = sarifLog.Runs[0].CreateWorkItemTitle();
            title.Should().EndWith(expected);

            // A logical location that's a path is truncated to it's file name
            expected = ":Warning]: TestRuleId: Test Rule (in '0123456789')";
            result.Locations[0].LogicalLocation.FullyQualifiedName = "ll" + new string('b', 1024) + "\\0123456789";
            title = sarifLog.Runs[0].CreateWorkItemTitle();
            title.Should().EndWith(expected);

            // A logical location that's a path is truncated using its full path
            expectedTemplate = "[aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:Warning]: TestRuleId: Test Rule (in 'll0123456789\\...')";
            expected = $":Warning]: TestRuleId: Test Rule (in 'll0123456789\\" + new string('c', maxLength - expectedTemplate.Length) + "...')";
            result.Locations[0].LogicalLocation.FullyQualifiedName = "ll0123456789\\" + new string('c', 1024);
            title = sarifLog.Runs[0].CreateWorkItemTitle();
            title.Should().EndWith(expected);
        }

        private class PhraseToolNamesTestCase
        {
            public PhraseToolNamesTestCase(List<string> input, string expectedOutput)
            {
                Input = input;
                ExpectedOutput = expectedOutput;
            }

            public List<string> Input { get; }
            public string ExpectedOutput { get; }
        }

        private static readonly ReadOnlyCollection<PhraseToolNamesTestCase> s_phraseToolNamesTestCases =
            new ReadOnlyCollection<PhraseToolNamesTestCase>(new PhraseToolNamesTestCase[]
            {
                new PhraseToolNamesTestCase(
                    input: new List<string>() {null},
                    expectedOutput: "''"),

                new PhraseToolNamesTestCase(
                    input: new List<string>() {""},
                    expectedOutput: "''"),

                new PhraseToolNamesTestCase(
                    input: new List<string>() {"CredentialScanner"},
                    expectedOutput: "'CredentialScanner'"),

                new PhraseToolNamesTestCase(
                    input: new List<string>() {"CredentialScanner", "Binskim"},
                    expectedOutput: "'CredentialScanner' and 'Binskim'"),

                new PhraseToolNamesTestCase(
                    input: new List<string>() {"SEMMLE", "CredentialScanner", "Binskim"},
                    expectedOutput: "'SEMMLE', 'CredentialScanner' and 'Binskim'"),

                new PhraseToolNamesTestCase(
                    input: new List<string>() {"SEMMLE", "", "Binskim"},
                    expectedOutput: "'SEMMLE' and 'Binskim'"),

                new PhraseToolNamesTestCase(
                    input: new List<string>() {"", "CredentialScanner", "SEMMLE", "Binskim"},
                    expectedOutput: "'CredentialScanner', 'SEMMLE' and 'Binskim'"),

                new PhraseToolNamesTestCase(
                    input: new List<string>() {"", "CredentialScanner"},
                    expectedOutput: "'CredentialScanner'"),
            });

        [Fact]
        public void SarifWorkItemExtensions_PhraseToolNames_ConstructPhraseCorrectly()
        {
            StringBuilder sb = new StringBuilder();

            foreach (PhraseToolNamesTestCase testCase in s_phraseToolNamesTestCases)
            {
                string actualOutput = testCase.Input.ToAndPhrase();

                bool succeeded = (testCase.ExpectedOutput == null && actualOutput == null)
                    || (actualOutput?.Equals(testCase.ExpectedOutput, StringComparison.Ordinal) == true);

                if (!succeeded)
                {
                    sb.AppendLine($"    Input: {Utilities.SafeFormat(string.Join(" ", testCase.Input.ToArray()))} Expected: {Utilities.SafeFormat(testCase.ExpectedOutput)} Actual: {Utilities.SafeFormat(actualOutput)}");
                }
            }

            sb.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{sb.ToString()}");
        }

        [Fact]
        public void SarifWorkItemExtensions_GetAggregateResultCount_ComputeResultCountsFromLogs()
        {
            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();

            int resultCount = sarifLog.GetAggregateFilableResultsCount();
            resultCount.Should().Be(1);

            sarifLog = TestData.CreateTwoRunThreeResultLog();
            resultCount = sarifLog.GetAggregateFilableResultsCount();
            resultCount.Should().Be(3);

            sarifLog = TestData.CreateEmptyRun();
            resultCount = sarifLog.GetAggregateFilableResultsCount();
            resultCount.Should().Be(0);
        }

        [Fact]
        public void SarifWorkItemExtensions_GetRunToolNames_FetchesAllRunToolNames()
        {
            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();

            List<string> toolNames = sarifLog.GetToolNames();
            toolNames.Count.Should().Be(1);
            toolNames.Should().Contain(TestData.TestToolName);

            sarifLog = TestData.CreateTwoRunThreeResultLog();
            toolNames = sarifLog.GetToolNames();
            toolNames.Count.Should().Be(2);
            toolNames.Should().Contain(TestData.TestToolName);
            toolNames.Should().Contain(TestData.SecondTestToolName);

            sarifLog = TestData.CreateEmptyRun();
            toolNames = sarifLog.GetToolNames();
            toolNames.Count.Should().Be(0);
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_SingleResultMultipleLocations()
        {
            string toolName = "TestToolName";
            string firstLocation = @"C:\Test\Data\File1.sarif";
            string additionLocationCount = "2";

            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();
            sarifLog.Runs[0].VersionControlProvenance = null;

            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });

            description.Should().BeEquivalentTo($"This work item contains 1 '{toolName}' issue(s) detected in {firstLocation} (+{additionLocationCount} locations). Click the 'Scans' tab to review results.");
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_MultipleResults()
        {
            string toolName = "TestToolName";
            string firstLocation = testSarifFilePath;
            int numOfResult = 15;
            string additionLocationCount = "14";

            int index = 1;
            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(0, numOfResult);
            sarifLog.Runs[0].Results.ForEach(r => r.Locations
                                               = new[]
                                               {
                                                   new Location
                                                   {
                                                       PhysicalLocation = new PhysicalLocation
                                                       {
                                                           ArtifactLocation = new ArtifactLocation
                                                           {
                                                               Uri = new Uri(string.Format(firstLocation, index++))
                                                           }
                                                       }
                                                   }
                                               });

            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });

            description.Should().BeEquivalentTo($"This work item contains {numOfResult} '{toolName}' issue(s) detected in {string.Format(firstLocation, 1)} (+{additionLocationCount} locations). Click the 'Scans' tab to review results.");
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_ResultsShouldNotBeFiled()
        {
            string toolName = "TestToolName";
            string firstLocation = testSarifFilePath;
            int numOfResult = 8;
            int expectedNumOfResult = 0;

            int index = 1;
            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(0, numOfResult);
            sarifLog.Runs[0].Results.ForEach(r => r.Locations
                                               = new[]
                                               {
                                                   new Location
                                                   {
                                                       PhysicalLocation = new PhysicalLocation
                                                       {
                                                           ArtifactLocation = new ArtifactLocation
                                                           {
                                                               Uri = new Uri(string.Format(firstLocation, index++))
                                                           }
                                                       }
                                                   }
                                               });

            sarifLog.Runs[0].Results.ForEach(r => r.BaselineState = BaselineState.Unchanged);
            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });

            description.Should().BeEquivalentTo($"This work item contains {expectedNumOfResult} '{toolName}' issue(s) detected in . Click the 'Scans' tab to review results.");
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_WithoutResults()
        {
            int numOfResult = 0;

            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(0, numOfResult);

            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });

            description.Should().BeEquivalentTo($"This work item contains {numOfResult} '' issue(s) detected in . Click the 'Scans' tab to review results.");
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_MultipleResultsMultipleLocations()
        {
            string toolName = "TestToolName";
            string firstLocation = testSarifFilePath;
            int numOfResult = 15;
            string additionLocationCount = "29"; // 15 results each results have 2 locations

            int index = 1;
            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(0, numOfResult);
            sarifLog.Runs[0].Results.ForEach(r => r.Locations
                                               = new[]
                                               {
                                                   new Location
                                                   {
                                                       PhysicalLocation = new PhysicalLocation
                                                       {
                                                           ArtifactLocation = new ArtifactLocation
                                                           {
                                                               Uri = new Uri(string.Format(firstLocation, index++))
                                                           }
                                                       }
                                                   },
                                                   new Location
                                                   {
                                                       PhysicalLocation = new PhysicalLocation
                                                       {
                                                           ArtifactLocation = new ArtifactLocation
                                                           {
                                                               Uri = new Uri(string.Format(firstLocation, index++))
                                                           }
                                                       }
                                                   }
                                               });

            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });

            description.Should().BeEquivalentTo($"This work item contains {numOfResult} '{toolName}' issue(s) detected in {string.Format(firstLocation, 1)} (+{additionLocationCount} locations). Click the 'Scans' tab to review results.");
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_SingleResultWithMultipleArtifacts()
        {
            string toolName = "TestToolName";
            string firstLocation = string.Format(testSarifFilePath, 1);
            string secondLocation = string.Format(testSarifFilePath, 2);
            string thirdLocation = string.Format(testSarifFilePath, 3);

            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(0, 1);
            sarifLog.Runs[0].Results[0].Locations = new[]
            {
                new Location
                {
                    PhysicalLocation = new PhysicalLocation
                    {
                        ArtifactLocation = new ArtifactLocation
                        {
                            Uri = new Uri(firstLocation),
                        }
                    }
                }
            };

            sarifLog.Runs[0].Results[0].RelatedLocations = new[]
            {
                new Location
                {
                    PhysicalLocation = new PhysicalLocation
                    {
                        ArtifactLocation = new ArtifactLocation
                        {
                            Uri = new Uri(secondLocation),
                        }
                    }
                }
            };

            sarifLog.Runs[0].Results[0].CodeFlows = new[]
            {
                new CodeFlow
                {
                    ThreadFlows = new[]
                    {
                        new ThreadFlow
                        {
                            Locations = new[]
                            {
                                new ThreadFlowLocation
                                {
                                    Location = new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                            ArtifactLocation = new ArtifactLocation
                                            {
                                                Uri = new Uri(thirdLocation),
                                            }
                                       }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            sarifLog.Runs[0].VersionControlProvenance = null;

            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });

            description.Should().BeEquivalentTo($"This work item contains 1 '{toolName}' issue(s) detected in {firstLocation}. Click the 'Scans' tab to review results.");
        }

        [Fact]
        public void SarifWorkItemExtensions_ShouldBeFiledTests()
        {
            var testCases = new[]
            {
                new { Result = (Result)null, Expected = false },
                new { Result = new Result { Kind = ResultKind.Pass }, Expected = false },
                new { Result = new Result { Kind = ResultKind.Fail }, Expected = true },
                new { Result = new Result { Kind = ResultKind.Open }, Expected = true },
                new { Result = new Result { Kind = ResultKind.Review }, Expected = true },
                new { Result = new Result { Kind = ResultKind.Informational }, Expected = false },
                new { Result = new Result { Kind = ResultKind.None }, Expected = false },
                new { Result = new Result { Kind = ResultKind.NotApplicable }, Expected = false },
                new { Result = new Result { Kind = ResultKind.Fail, BaselineState = BaselineState.Absent }, Expected = false },
                new { Result = new Result { Kind = ResultKind.Open, BaselineState = BaselineState.Updated }, Expected = false },
                new { Result = new Result { Kind = ResultKind.Review, BaselineState = BaselineState.Unchanged }, Expected = false },
                new { Result = new Result { Kind = ResultKind.Fail, BaselineState = BaselineState.New }, Expected = true },
                new { Result = new Result { Kind = ResultKind.Fail, BaselineState = BaselineState.None }, Expected = true },
                new { Result = new Result { Kind = ResultKind.Fail, Suppressions = new [] { new Suppression { Status = SuppressionStatus.Accepted } } }, Expected = false },
                new { Result = new Result { Kind = ResultKind.Fail, Suppressions = new [] { new Suppression { Status = SuppressionStatus.Rejected } } }, Expected = false },
                new { Result = new Result { Kind = ResultKind.Fail, Suppressions = new Suppression[] {  } }, Expected = true },
                new { Result = new Result { Kind = ResultKind.Fail, Suppressions = new [] { new Suppression { Status = SuppressionStatus.Rejected }, new Suppression { Status = SuppressionStatus.UnderReview } } }, Expected = false },
            };

            foreach (var test in testCases)
            {
                bool actual = test.Result.ShouldBeFiled();
                actual.Should().Be(actual);
            }
        }

        private static readonly string ToolName = Guid.NewGuid().ToString();

        public Tuple<string, Result>[] ResultsWithVariousRuleExpressions = new[]
        {
            new Tuple<string, Result>("Result with rule id only", new Result
            {
                RuleId = TestRuleId,
                Locations = new []
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri("https://" + Guid.NewGuid().ToString())
                            }
                        }
                    }
                }
            }),
            new Tuple<string, Result>("Result with rule.Id", new Result
            {
                Rule = new ReportingDescriptorReference
                {
                    Id = TestRuleId
                }
            }),
            new Tuple<string, Result>("Result with rule index only", new Result
            {
                RuleIndex = 0
            }),
            new Tuple<string, Result>("Result with rule index only", new Result
            {
                Rule = new ReportingDescriptorReference
                {
                    Index = 0
                }
            })
        };

        private const string TestRuleId = nameof(TestRuleId);

        private static SarifLog CreateLogWithEmptyRun()
        {
            return new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = ToolName,
                                Rules = new ReportingDescriptor[]
                                {
                                    new ReportingDescriptor
                                    {
                                        Name = "Test Rule",
                                        Id = nameof(TestRuleId)
                                    }
                                }
                            }
                        },
                        Results = new List<Result>()
                    }
                }
            };
        }
    }
}
