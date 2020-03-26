// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
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
        public void SarifWorkItemExtensions_ComputeToolResultCounts_CountsSingleResult()
        {
            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();
            Dictionary<Run, int> resultsCounts = sarifLog.ComputeRunResultCounts();
            resultsCounts.Keys.Should().HaveCount(1);
            resultsCounts.Keys.Should().Contain(sarifLog.Runs[0]);
            resultsCounts[sarifLog.Runs[0]].Should().Be(1);
        }

        [Fact]
        public void SarifWorkItemExtensions_ComputeToolResultCounts_CountsMultipleToolsMultipleResults()
        {
            SarifLog sarifLog = TestData.CreateTwoRunThreeResultLog();
            Dictionary<Run, int> resultsCounts = sarifLog.ComputeRunResultCounts();
            resultsCounts.Keys.Should().HaveCount(2);
            resultsCounts.Keys.Should().Contain(sarifLog.Runs[0]);
            resultsCounts.Keys.Should().Contain(sarifLog.Runs[1]);
            resultsCounts[sarifLog.Runs[0]].Should().Be(2);
            resultsCounts[sarifLog.Runs[1]].Should().Be(1);
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
