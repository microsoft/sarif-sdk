using System;
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling
{
    public class FilteringStrategyFactoryTests
    {
        [Fact]
        public void CreateFilteringStrategy_ThrowsIfKindIsNone()
        {
            Action action = () => FilteringStrategyFactory.CreateFilteringStrategy(FilteringStrategyKind.None);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void CreateFilteringStrategy_ThrowsIfKindIsNotAMemberOfTheEnumeration()
        {
            Action action = () => FilteringStrategyFactory.CreateFilteringStrategy((FilteringStrategyKind)(-1));

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void CreateFilteringStrategy_CreatesAllResultsFilteringStrategy()
        {
            var filteringStrategy = FilteringStrategyFactory.CreateFilteringStrategy(FilteringStrategyKind.AllResults);

            filteringStrategy.Should().BeOfType<AllResultsFilteringStrategy>();
        }

        [Fact]
        public void CreateFilteringStrategy_CreatesNewResultsFilteringStrategy()
        {
            var filteringStrategy = FilteringStrategyFactory.CreateFilteringStrategy(FilteringStrategyKind.NewResults);

            filteringStrategy.Should().BeOfType<NewResultsFilteringStrategy>();
        }
    }
}
