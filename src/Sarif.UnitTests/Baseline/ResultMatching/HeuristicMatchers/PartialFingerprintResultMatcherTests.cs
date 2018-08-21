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
            MatchingResult resultA = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null) };

            MatchingResult resultB = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null) };

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new List<MatchingResult>() { resultA }, new List<MatchingResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void PartialFingerprintResultMatcher_DifferentPartialFingerprints_DoesNotMatch()
        {
            MatchingResult resultA = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null) };

            resultA.Result.PartialFingerprints = new Dictionary<string, string>() { { "Fingerprint1", "Value1" } };

            MatchingResult resultB = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null) };
            
            resultA.Result.PartialFingerprints = new Dictionary<string, string>() { { "Fingerprint1", "Value2" } };

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new List<MatchingResult>() { resultA }, new List<MatchingResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void PartialFingerprintResultMatcher_SamePartialFingerprints_Matches()
        {
            MatchingResult resultA = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test1", "file://test2", null) };

            resultA.Result.PartialFingerprints = new Dictionary<string, string>() { { "Fingerprint1", "Value1" } };

            MatchingResult resultB = new MatchingResult() { Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test3", "file://test4", null) };
            
            resultA.Result.PartialFingerprints = new Dictionary<string, string>() { { "Fingerprint1", "Value1" } };

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new List<MatchingResult>() { resultA }, new List<MatchingResult>() { resultB });

            matchedResults.Should().BeEmpty();
        }
    }
}
