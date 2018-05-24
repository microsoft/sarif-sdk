// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    internal class IdenticalResultMatcher : IResultMatcher
    {
        public IEnumerable<MatchedResults> MatchResults(IEnumerable<MatchingResult> baseline, IEnumerable<MatchingResult> current)
        {
            List<MatchedResults> matchedResults = new List<MatchedResults>();
            Dictionary<Result, MatchingResult> baselineResults = new Dictionary<Result, MatchingResult>(ResultEqualityComparer.Instance);

            foreach (var result in baseline)
            {
                baselineResults.Add(result.Result, result);
            }

            foreach (var result in current)
            {
                if (baselineResults.ContainsKey(result.Result))
                {
                    matchedResults.Add(new MatchedResults() { BaselineResult = baselineResults[result.Result], CurrentResult = result, MatchingAlgorithm = this });
                }
            }

            return matchedResults;
        }
    }
}
