// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Baseline
{
    public class ExtractedResultTests
    {
        [Fact]
        public void WhatComparer_MatchesCategory()
        {
            Run run = new Run()
            {
                Tool = new Tool()
                {
                    Driver = new ToolComponent()
                    {
                        Rules = new List<ReportingDescriptor>()
                        {
                            new ReportingDescriptor()
                            {
                                Id = "Rule001"
                            },
                            new ReportingDescriptor()
                            {
                                Id = "Rule002"
                            },
                            new ReportingDescriptor()
                            {
                                Id = "Rule001_v2",
                                DeprecatedIds = new List<string>() { "Rule001" }
                            }
                        }
                    }
                }
            };

            ExtractedResult left;
            ExtractedResult right;

            // Same RuleIndex: Matches
            left = new ExtractedResult(new Result() { RuleIndex = 0 }, run);
            right = new ExtractedResult(new Result() { RuleIndex = 0 }, run);
            left.MatchesCategory(right).Should().BeTrue();
            right.MatchesCategory(left).Should().BeTrue();

            // RuleId from index matches RuleId: Matches
            left = new ExtractedResult(new Result() { RuleId = "Rule001" }, run);
            left.MatchesCategory(right).Should().BeTrue();
            right.MatchesCategory(left).Should().BeTrue();

            // Different RuleId: Non-Match
            left = new ExtractedResult(new Result() { RuleIndex = 1 }, run);
            left.MatchesCategory(right).Should().BeFalse();
            right.MatchesCategory(left).Should().BeFalse();

            // RuleId is a DeprecatedId from other Rule: Matches
            left = new ExtractedResult(new Result() { RuleIndex = 2 }, run);
            left.MatchesCategory(right).Should().BeTrue();
            right.MatchesCategory(left).Should().BeTrue();

            // DeprecatedIds, but they don't match
            right = new ExtractedResult(new Result() { RuleIndex = 1 }, run);
            left.MatchesCategory(right).Should().BeFalse();
            right.MatchesCategory(left).Should().BeFalse();
        }
    }
}
