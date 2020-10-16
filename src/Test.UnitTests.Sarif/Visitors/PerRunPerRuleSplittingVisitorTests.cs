// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class PerRunPerRuleSplittingVisitorTests
    {
        [Fact]
        public void PerRunPerRuleSplittingVisitor_EmptyLog()
        {
            var visitor = new PerRunPerRuleSplittingVisitor();
            visitor.VisitRun(new Run());
            visitor.SplitSarifLogs.Count.Should().Be(0);
        }

        [Fact]
        public void PerRunPerRuleSplittingVisitor_RetainsNewResultsOnly()
        {
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
                                RuleId = TestData.RuleIds.Rule1,
                                BaselineState = BaselineState.New
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule2,
                                BaselineState = BaselineState.Updated
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule2,
                                BaselineState = BaselineState.New
                            }
                        }
                    }
                }
            };

            var visitor = new PerRunPerRuleSplittingVisitor();
            visitor.VisitSarifLog(sarifLog);

            visitor.SplitSarifLogs.Count.Should().Be(2);

            visitor.SplitSarifLogs[0].Runs[0].Results.Count.Should().Be(1);
            visitor.SplitSarifLogs[0].Runs[0].Results[0].RuleId.Should().Be(TestData.RuleIds.Rule1);

            visitor.SplitSarifLogs[1].Runs[0].Results.Count.Should().Be(1);
            visitor.SplitSarifLogs[1].Runs[0].Results[0].RuleId.Should().Be(TestData.RuleIds.Rule2);
        }

        [Fact]
        public void PerRunPerRuleSplittingVisitor_RetainsRulesForUnbaselinedAndNewResultsOnly()
        {
            SarifLog sarifLog = GetTestSarifLog();

            HashSet<string> ruleIds = new HashSet<string>();

            for (int i = 0; i < sarifLog.Runs[0].Results.Count; i++)
            {
                Result result = sarifLog.Runs[0].Results[i];
                if (result.BaselineState == BaselineState.New ||
                    result.BaselineState == BaselineState.None)
                {
                    ruleIds.Add(result.RuleId);
                }
            }

            int ruleCount = ruleIds.Count;

            var visitor = new PerRunPerRuleSplittingVisitor();
            visitor.VisitSarifLog(sarifLog);

            visitor.SplitSarifLogs.Count.Should().Be(ruleCount);
        }

        private SarifLog GetTestSarifLog()
        {
            return new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Results = new Result[]
                        {
                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule1
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule2,
                                BaselineState = BaselineState.Absent
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule3,
                                BaselineState = BaselineState.New
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule3,
                                BaselineState = BaselineState.Updated
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule3,
                                BaselineState = BaselineState.New
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule4,
                                BaselineState = BaselineState.None
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule5,
                                BaselineState = BaselineState.Unchanged
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule5,
                                BaselineState = BaselineState.Unchanged
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule6,
                                BaselineState = BaselineState.Updated
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule7,
                                BaselineState = BaselineState.New
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule8,
                                BaselineState = BaselineState.New
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule9,
                                BaselineState = BaselineState.New
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule10,
                                BaselineState = BaselineState.New
                            }
                        }
                    }
                }
            };
        }
    }
}
