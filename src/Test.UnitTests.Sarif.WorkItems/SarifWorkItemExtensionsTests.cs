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
        private static readonly string s_testSarifFilePathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Test", "Data", "File{0}.sarif");

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
            result.Locations[0].LogicalLocation.FullyQualifiedName = "ll" + new string('b', 1024) + Path.DirectorySeparatorChar + "0123456789";
            title = sarifLog.Runs[0].CreateWorkItemTitle();
            title.Should().EndWith(expected);

            // A logical location that's a path is truncated using its full path
            expectedTemplate = "[aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:Warning]: TestRuleId: Test Rule (in 'll0123456789" + Path.DirectorySeparatorChar + "...')";
            expected = $":Warning]: TestRuleId: Test Rule (in 'll0123456789" + Path.DirectorySeparatorChar + new string('c', maxLength - expectedTemplate.Length) + "...')";
            result.Locations[0].LogicalLocation.FullyQualifiedName = "ll0123456789" + Path.DirectorySeparatorChar + new string('c', 1024);
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
            string toolName = TestData.TestToolName;
            int additionLocationCount = 2;
            int expectedResultCount = 1;
            string multipleToolsFooter = string.Empty;

            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();
            sarifLog.Runs[0].VersionControlProvenance = null;

            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });

            // This work item contains {0} {1} issue(s) detected in {2}{3}. Click the 'Scans' tab to review results.
            description.Should().BeEquivalentTo(
                string.Format(
                    WorkItemsResources.WorkItemBodyTemplateText,
                    expectedResultCount,
                    $"'{toolName}'",
                    $"{TestData.FileLocations.Location1} (+{additionLocationCount} locations)",
                    multipleToolsFooter));
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_MultipleResults()
        {
            string toolName = TestData.TestToolName;
            string firstLocation = s_testSarifFilePathTemplate;
            int numOfResult = 15;
            int additionLocationCount = 14;
            string multipleToolsFooter = string.Empty;

            int index = 1;
            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(0, numOfResult);
            sarifLog.Runs[0].Results.ForEach(
                r => r.Locations
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

            // This work item contains {0} {1} issue(s) detected in {2}{3}. Click the 'Scans' tab to review results.
            description.Should().BeEquivalentTo(
                string.Format(
                    WorkItemsResources.WorkItemBodyTemplateText,
                    numOfResult,
                    $"'{toolName}'",
                    string.Format(SarifWorkItemFiler.s_multipleLocationsTextPattern, string.Format(firstLocation, 1), additionLocationCount),
                    multipleToolsFooter));
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_ResultsShouldNotBeFiled()
        {
            string multipleToolsFooter = string.Empty;
            string firstLocation = string.Format(s_testSarifFilePathTemplate, 1);
            int numOfResult = 8;

            int index = 1;
            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(ruleIdStartIndex: 0, numOfResult);

            foreach (Result result in sarifLog.Runs[0].Results)
            {
                result.Locations = new[]
                {
                    new Location { PhysicalLocation = new PhysicalLocation { ArtifactLocation = new ArtifactLocation { Uri = new Uri(string.Format(firstLocation, index++)) } } }
                };
                result.BaselineState = BaselineState.Unchanged;
            }

            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });
            description.Should().BeNull();
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_WithoutResults()
        {
            string multipleToolsFooter = string.Empty;
            int numOfResult = 0;

            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(0, numOfResult);

            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });
            description.Should().BeNull();
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_MultipleResultsMultipleLocations()
        {
            string toolName = TestData.TestToolName;
            string multipleToolsFooter = string.Empty;
            string firstLocation = s_testSarifFilePathTemplate;
            int numOfResult = 15;
            string additionLocationCount = "29"; // 15 results each results have 2 locations

            int index = 1;
            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(0, numOfResult);
            sarifLog.Runs[0].Results.ForEach(
                r => r.Locations = new[]
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

            // This work item contains {0} {1} issue(s) detected in {2}{3}. Click the 'Scans' tab to review results.
            description.Should().BeEquivalentTo(
                string.Format(
                    WorkItemsResources.WorkItemBodyTemplateText,
                    numOfResult,
                    $"'{toolName}'",
                    string.Format(SarifWorkItemFiler.s_multipleLocationsTextPattern, string.Format(firstLocation, 1), additionLocationCount),
                    multipleToolsFooter));
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_SingleResultWithMultipleArtifacts()
        {
            string toolName = TestData.TestToolName;
            int numOfResult = 1;
            string multipleToolsFooter = string.Empty;
            string firstLocation = string.Format(s_testSarifFilePathTemplate, 1);
            string secondLocation = string.Format(s_testSarifFilePathTemplate, 2);
            string thirdLocation = string.Format(s_testSarifFilePathTemplate, 3);

            SarifLog sarifLog = TestData.CreateSimpleLogWithRules(0, numOfResult);
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

            // This work item contains {0} {1} issue(s) detected in {2}{3}. Click the 'Scans' tab to review results.
            description.Should().BeEquivalentTo(
                string.Format(
                    WorkItemsResources.WorkItemBodyTemplateText,
                    numOfResult,
                    $"'{toolName}'",
                    firstLocation,
                    multipleToolsFooter));
        }

        [Fact]
        public void SarifWorkItemExtensions_CreateWorkItemDescription_MultipleRunsDifferenceTools()
        {
            string toolName1 = TestData.TestToolName;
            string toolName2 = TestData.SecondTestToolName;
            int numOfResult = 3;
            int additionLocationCount = 2;
            string multipleToolsFooter = WorkItemsResources.MultipleToolsFooter;

            SarifLog sarifLog = TestData.CreateTwoRunThreeResultLog();

            string description = sarifLog.CreateWorkItemDescription(new SarifWorkItemContext() { CurrentProvider = Microsoft.WorkItems.FilingClient.SourceControlProvider.AzureDevOps });

            // This work item contains {0} {1} issue(s) detected in {2}{3}. Click the 'Scans' tab to review results.
            description.Should().BeEquivalentTo(
                string.Format(
                    WorkItemsResources.WorkItemBodyTemplateText,
                    numOfResult,
                    $"'{toolName1}' and '{toolName2}'",
                    string.Format(SarifWorkItemFiler.s_multipleLocationsTextPattern, TestData.FileLocations.Location1, additionLocationCount),
                    multipleToolsFooter));
        }

        [Fact]
        public void SarifWorkItemExtensions_ShouldBeFiledTests()
        {
            var testCases = new[]
            {
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

            // test case for null result
            Result nullResult = null;
            Assert.Throws<NullReferenceException>(() => nullResult.ShouldBeFiled());
            (nullResult?.ShouldBeFiled()).Should().BeNull();
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
