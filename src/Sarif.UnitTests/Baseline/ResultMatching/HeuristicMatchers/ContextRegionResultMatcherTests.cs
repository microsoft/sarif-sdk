// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    public class ContextRegionResultMatcherTests
    {
        private static ContextRegionHeuristicMatcher matcher = new ContextRegionHeuristicMatcher();

        [Fact]
        public void ContextRegionHeuristicMatcher_NoRegion_DoesNotMatchResults()
        {
            MatchingResult resultA = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null, null) };

            MatchingResult resultB = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null, null) };

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new List<MatchingResult>() { resultA }, new List<MatchingResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void ContextRegionHeuristicMatcher_DifferentRegion_DoesNotMatchResults()
        {
            MatchingResult resultA = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null, "test one") };

            MatchingResult resultB = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null, "test two") };

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new List<MatchingResult>() { resultA }, new List<MatchingResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }


        [Fact]
        public void ContextRegionHeuristicMatcher_SameRegion_MatchesResults()
        {
            MatchingResult resultA = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null, "region contents") };

            MatchingResult resultB = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null, "region contents") };

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new List<MatchingResult>() { resultA }, new List<MatchingResult>() { resultB });

            matchedResults.Should().HaveCount(1);
        }
    }
}
