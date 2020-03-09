// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    public class FullFingerprintMatcherTests
    {
        private static readonly FullFingerprintResultMatcher matcher = new FullFingerprintResultMatcher();

        [Fact]
        public void FullFingerprintMatcher_MatchesIdenticalFingerprints()
        {
            Result resultA = ResultMatchingTestHelpers.CreateMatchingResult(@"http://testtesttest", @"file://testa", "contextual contexty contexts");
            Result resultB = ResultMatchingTestHelpers.CreateMatchingResult(@"http://notasmuchatest", @"file://differentpath", "different contexty contexts");

            resultA.Fingerprints = new Dictionary<string, string>() { { "FingerprintAlgorithm1", "FingerprintValue1" }, { "FingerprintAlgorithm2", "FingerprintValue2" } };
            resultB.Fingerprints = new Dictionary<string, string>() { { "FingerprintAlgorithm1", "FingerprintValue1" } };

            ExtractedResult matchingResultA = new ExtractedResult(resultA, null);
            ExtractedResult matchingResultB = new ExtractedResult(resultB, null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { matchingResultA }, new ExtractedResult[] { matchingResultB });

            matchedResults.Should().HaveCount(1);
            matchedResults.First().PreviousResult.Should().BeEquivalentTo(matchingResultA);
            matchedResults.First().CurrentResult.Should().BeEquivalentTo(matchingResultB);
        }

        [Fact]
        public void FullFingerprintMatcher_DoesNotMatchOnChangedFingerprints()
        {
            Result resultA = ResultMatchingTestHelpers.CreateMatchingResult(@"http://testtesttest", @"file://testa", "contextual contexty contexts");
            Result resultB = ResultMatchingTestHelpers.CreateMatchingResult(@"http://testtesttest", @"file://testa", "contextual contexty contexts");

            resultA.Fingerprints = new Dictionary<string, string>() { { "FingerprintAlgorithm1", "FingerprintValue1" }, { "FingerprintAlgorithm2", "FingerprintValue2" } };
            resultB.Fingerprints = new Dictionary<string, string>() { { "FingerprintAlgorithm1", "FingerprintValue3" }, { "FingerprintAlgorithm2", "FingerprintValue4" } };

            ExtractedResult matchingResultA = new ExtractedResult(resultA, null);
            ExtractedResult matchingResultB = new ExtractedResult(resultB, null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { matchingResultA }, new ExtractedResult[] { matchingResultB });

            matchedResults.Should().HaveCount(0);
        }
    }
}
