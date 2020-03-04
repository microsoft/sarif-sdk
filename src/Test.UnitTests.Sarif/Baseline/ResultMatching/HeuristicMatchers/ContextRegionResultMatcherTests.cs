// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    public class ContextRegionResultMatcherTests
    {
        private static readonly ContextRegionHeuristicMatcher matcher = new ContextRegionHeuristicMatcher();

        [Fact]
        public void ContextRegionHeuristicMatcher_NoRegion_DoesNotMatchResults()
        {
            ExtractedResult resultA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null, null), null);
            ExtractedResult resultB = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null, null), null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new List<ExtractedResult>() { resultA }, new List<ExtractedResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void ContextRegionHeuristicMatcher_DifferentRegion_DoesNotMatchResults()
        {
            ExtractedResult resultA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null, "test one"), null);
            ExtractedResult resultB = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null, "test two"), null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new List<ExtractedResult>() { resultA }, new List<ExtractedResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }


        [Fact]
        public void ContextRegionHeuristicMatcher_SameRegion_MatchesResults()
        {
            ExtractedResult resultA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null, "region contents"), null);
            ExtractedResult resultB = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null, "region contents"), null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new List<ExtractedResult>() { resultA }, new List<ExtractedResult>() { resultB });

            matchedResults.Should().HaveCount(1);
        }
    }
}
