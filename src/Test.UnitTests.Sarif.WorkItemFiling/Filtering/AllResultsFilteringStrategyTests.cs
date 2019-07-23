// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Filtering;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling.Filtering
{
    public class AllResultsFilteringStrategyTests
    {
        [Fact]
        public void AllResultsFilteringStrategy_RequiresResults()
        {
            var strategy = new AllResultsFilteringStrategy();

            Action action = () => strategy.FilterResults(results: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AllResultsFilteringStrategy_AcceptsEmptyResults()
        {
            var strategy = new AllResultsFilteringStrategy();

            IList<Result> filteredResults = strategy.FilterResults(new Result[0]);

            filteredResults.Count.Should().Be(0);
        }

        [Fact]
        public void AllResultsFilteringStrategy_SelectsAllResults()
        {
            var strategy = new AllResultsFilteringStrategy();
            var results = new Result[]
            {
                new Result
                {
                    RuleId = "TST0001",
                },

                new Result
                {
                    RuleId = "TST0002",
                    BaselineState = BaselineState.Absent
                },

                new Result
                {
                    RuleId = "TST0003",
                    BaselineState = BaselineState.New
                },

                new Result
                {
                    RuleId = "TST0004",
                    BaselineState = BaselineState.None
                },

                new Result
                {
                    RuleId = "TST0005",
                    BaselineState = BaselineState.Unchanged
                },

                new Result
                {
                    RuleId = "TST0006",
                    BaselineState = BaselineState.Updated
                },

                new Result
                {
                    RuleId = "TST0007",
                    BaselineState = BaselineState.New
                }
            };

            IList<Result> filteredResults = strategy.FilterResults(results);

            int numResults = results.Length;
            filteredResults.Count.Should().Be(numResults);
            for (int i = 0; i < numResults; ++i)
            {
                filteredResults[i].RuleId.Should().Be("TST000" + (i + 1).ToString());
            }
        }
    }
}
