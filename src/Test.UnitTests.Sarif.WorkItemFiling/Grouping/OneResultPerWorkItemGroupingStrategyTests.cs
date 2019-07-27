// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
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

            IList<WorkItemFilingMetadata> metadata = strategy.GroupResults(new Result[0]);

            metadata.Count.Should().Be(0);
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

            IList<WorkItemFilingMetadata> metadata = strategy.GroupResults(results);

            metadata.Count.Should().Be(2);
            metadata[0].Results.Count.Should().Be(1);
            metadata[0].Results[0].RuleId.Should().Be("TST0001");
            metadata[1].Results.Count.Should().Be(1);
            metadata[1].Results[0].RuleId.Should().Be("TST0002");
        }
    }
}
