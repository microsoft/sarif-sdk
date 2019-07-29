// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling
{
    public class NewResultsFilteringStrategyTests
    {
        [Fact]
        public void NewResultsFilteringStrategy_RequiresResults()
        {
            var strategy = new NewResultsFilteringStrategy();

            Action action = () => strategy.FilterResults(results: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void NewResultsFilteringStrategy_AcceptsEmptyResults()
        {
            var strategy = new NewResultsFilteringStrategy();

            IList<Result> filteredResults = strategy.FilterResults(new Result[0]);

            filteredResults.Count.Should().Be(0);
        }

        [Fact]
        public void AllResultsFilteringStrategy_SelectsOnlyNewResults()
        {
            var strategy = new NewResultsFilteringStrategy();
            var results = new Result[]
            {
                new Result
                {
                    RuleId = TestConstants.RuleIds.Rule1
                },

                new Result
                {
                    RuleId = TestConstants.RuleIds.Rule2,
                    BaselineState = BaselineState.Absent
                },

                new Result
                {
                    RuleId = TestConstants.RuleIds.Rule3,
                    BaselineState = BaselineState.New
                },

                new Result
                {
                    RuleId = TestConstants.RuleIds.Rule4,
                    BaselineState = BaselineState.None
                },

                new Result
                {
                    RuleId = TestConstants.RuleIds.Rule5,
                    BaselineState = BaselineState.Unchanged
                },

                new Result
                {
                    RuleId = TestConstants.RuleIds.Rule6,
                    BaselineState = BaselineState.Updated
                },

                new Result
                {
                    RuleId = TestConstants.RuleIds.Rule7,
                    BaselineState = BaselineState.New
                }
            };

            IList<Result> filteredResults = strategy.FilterResults(results);

            filteredResults.Count.Should().Be(2);
            filteredResults[0].RuleId.Should().Be(TestConstants.RuleIds.Rule3);
            filteredResults[1].RuleId.Should().Be(TestConstants.RuleIds.Rule7);
        }
    }
}
