using System;
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling
{
    public class GroupingStrategyFactoryTests
    {
        [Fact]
        public void CreateGroupingStrategy_ThrowsIfKindIsNone()
        {
            Action action = () => GroupingStrategyFactory.CreateGroupingStrategy(GroupingStrategyKind.None);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void CreateFilteringStrategy_ThrowsIfKindIsNotAMemberOfTheEnumeration()
        {
            Action action = () => GroupingStrategyFactory.CreateGroupingStrategy((GroupingStrategyKind)(-1));

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void CreateFilteringStrategy_CreatesAllResultsFilteringStrategy()
        {
            var groupingStrategy = GroupingStrategyFactory.CreateGroupingStrategy(GroupingStrategyKind.PerResult);

            groupingStrategy.Should().BeOfType<OneResultPerWorkItemGroupingStrategy>();
        }
    }
}
