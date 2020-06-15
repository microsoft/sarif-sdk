// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemExtensionsTests
    {
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
            int maxLength = 256;
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
            int maxLength = 256;
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
            var context = new SarifWorkItemContext();
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
            var context = new SarifWorkItemContext();
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
