// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    public class FullFingerprintMatcherTests
    {
        private static FullFingerprintResultMatcher matcher = new FullFingerprintResultMatcher();
        
        [Fact]
        public void FullFingerprintMatcher_MatchesIdenticalFingerprints()
        {
            Result resultA = IdenticalResultMatcherTests.CreateMatchingResult(@"http://testtesttest", @"file://testa", "contextual contexty contexts");
            Result resultB = IdenticalResultMatcherTests.CreateMatchingResult(@"http://notasmuchatest", @"file://differentpath", "different contexty contexts");

            resultA.Fingerprints = new Dictionary<string, string>() { { "FingerprintAlgorithm1", "FingerprintValue1" }, { "FingerprintAlgorithm2", "FingerprintValue2" } };
            resultB.Fingerprints = new Dictionary<string, string>() { { "FingerprintAlgorithm1", "FingerprintValue1" }, { "FingerprintAlgorithm2", "FingerprintValue2" } };

            MatchingResult matchingResultA = new MatchingResult()
            {
                Result = resultA
            };
            MatchingResult matchingResultB = new MatchingResult()
            {
                Result = resultB
            };
        

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new MatchingResult[] { matchingResultA }, new MatchingResult[] { matchingResultB });

            matchedResults.Should().HaveCount(1);
            matchedResults.First().BaselineResult.ShouldBeEquivalentTo(matchingResultA);
            matchedResults.First().CurrentResult.ShouldBeEquivalentTo(matchingResultB);
        }
        
        [Fact]
        public void FullFingerprintMatcher_DoesNotMatchOnChangedFingerprints()
        {
            Result resultA = IdenticalResultMatcherTests.CreateMatchingResult(@"http://testtesttest", @"file://testa", "contextual contexty contexts");
            Result resultB = IdenticalResultMatcherTests.CreateMatchingResult(@"http://testtesttest", @"file://testa", "contextual contexty contexts");

            resultA.Fingerprints = new Dictionary<string, string>() { { "FingerprintAlgorithm1", "FingerprintValue1" }, { "FingerprintAlgorithm2", "FingerprintValue2" } };
            resultB.Fingerprints = new Dictionary<string, string>() { { "FingerprintAlgorithm1", "FingerprintValue3" }, { "FingerprintAlgorithm2", "FingerprintValue4" } };

            MatchingResult matchingResultA = new MatchingResult()
            {
                Result = resultA
            };
            MatchingResult matchingResultB = new MatchingResult()
            {
                Result = resultB
            };


            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new MatchingResult[] { matchingResultA }, new MatchingResult[] { matchingResultB });

            matchedResults.Should().HaveCount(0);
        }
    }
}
