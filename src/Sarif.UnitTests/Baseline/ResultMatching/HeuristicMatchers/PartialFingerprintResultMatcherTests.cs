// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    public class PartialFingerprintResultMatcherTests
    {
        private static PartialFingerprintResultMatcher matcher = new PartialFingerprintResultMatcher();

        [Fact]
        public void PartialFingerprintResultMatcher_WithoutPartialFingerprints_DoesNotMatch()
        {
            ExtractedResult resultA = new ExtractedResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null) };

            ExtractedResult resultB = new ExtractedResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null) };

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new List<ExtractedResult>() { resultA }, new List<ExtractedResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void PartialFingerprintResultMatcher_DifferentPartialFingerprints_DoesNotMatch()
        {
            ExtractedResult resultA = new ExtractedResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null) };

            resultA.Result.PartialFingerprints = new Dictionary<string, string>() { { "Fingerprint1", "Value1" } };

            ExtractedResult resultB = new ExtractedResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null) };
            
            resultA.Result.PartialFingerprints = new Dictionary<string, string>() { { "Fingerprint1", "Value2" } };

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new List<ExtractedResult>() { resultA }, new List<ExtractedResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void PartialFingerprintResultMatcher_SamePartialFingerprints_Matches()
        {
            ExtractedResult resultA = new ExtractedResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null) };

            resultA.Result.PartialFingerprints = new Dictionary<string, string>() { { "Fingerprint1", "Value1" } };

            ExtractedResult resultB = new ExtractedResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null) };
            
            resultA.Result.PartialFingerprints = new Dictionary<string, string>() { { "Fingerprint1", "Value1" } };

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new List<ExtractedResult>() { resultA }, new List<ExtractedResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }
    }
}
