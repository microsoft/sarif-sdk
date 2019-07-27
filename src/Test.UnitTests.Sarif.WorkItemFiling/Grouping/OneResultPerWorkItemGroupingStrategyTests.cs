// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling
{
    public class OneResultPerWorkItemGroupingStrategyTests
    {
        [Fact]
        public void OneResultPerWorkItemGroupingStrategy_RequiresResults()
        {
            var strategy = new OneResultPerWorkItemGroupingStrategy();

            Action action = () => strategy.GroupResults(results: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void OneResultPerWorkItemGroupingStrategy_AcceptsEmptyResults()
        {
            var strategy = new OneResultPerWorkItemGroupingStrategy();

            IList<WorkItemFilingMetadata> resultGroups = strategy.GroupResults(new Result[0]);

            resultGroups.Count.Should().Be(0);
        }

        [Fact]
        public void OneResultPerWorkItemGroupingStrategy_ProducesOneGroupPerResult()
        {
            var strategy = new OneResultPerWorkItemGroupingStrategy();
            var results = new Result[]
            {
                new Result
                {
                    RuleId = "TST0001",
                },

                new Result
                {
                    RuleId = "TST0002",
                }
            };

            IList<WorkItemFilingMetadata> resultGroups = strategy.GroupResults(results);

            resultGroups.Count.Should().Be(2);
            resultGroups[0].Results.Count.Should().Be(1);
            resultGroups[0].Results[0].RuleId.Should().Be("TST0001");
            resultGroups[1].Results.Count.Should().Be(1);
            resultGroups[1].Results[0].RuleId.Should().Be("TST0002");
        }
    }
}
