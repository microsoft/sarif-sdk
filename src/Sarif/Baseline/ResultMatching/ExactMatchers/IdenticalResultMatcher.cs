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
            Dictionary<Result, MatchingResult> baselineResults = new Dictionary<Result, MatchingResult>(IdenticalResultEqualityComparer.Instance);

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

        /// <summary>
        /// We want to mask out the fields that may be set during baselining, as they will not be equivalent during a comparison.
        /// </summary>
        public class IdenticalResultEqualityComparer : IEqualityComparer<Result>
        {
            public static readonly IdenticalResultEqualityComparer Instance = new IdenticalResultEqualityComparer();

            public bool Equals(Result x, Result y)
            {
                return ResultEqualityComparer.Instance.Equals(CreateMaskedResult(x), CreateMaskedResult(y));
            }

            public int GetHashCode(Result obj)
            {
                return ResultEqualityComparer.Instance.GetHashCode(CreateMaskedResult(obj));
            }

            public Result CreateMaskedResult(Result result)
            {
                Result masked = result.DeepClone();
                masked.Id = null;
                masked.SuppressionStates = SuppressionStates.None;
                masked.BaselineState = BaselineState.None;
                return masked;
            }

        }

    }
}
