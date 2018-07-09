// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    /// <summary>
    /// Matches two results if every part of the result is identical, except the fields used in baselining.
    /// 
    /// If a run contains multiple identical results, we will match them in order.
    /// </summary>
    class IdenticalResultMatcher : IResultMatcher
    {
        public IEnumerable<MatchedResults> MatchResults(IEnumerable<MatchingResult> baseline, IEnumerable<MatchingResult> current)
        {
            List<MatchedResults> matchedResults = new List<MatchedResults>();
            Dictionary<Result, List<MatchingResult>> baselineResults = new Dictionary<Result, List<MatchingResult>>(IdenticalResultEqualityComparer.Instance);

            foreach (var result in baseline)
            {
                if (!baselineResults.ContainsKey(result.Result) || baselineResults[result.Result] == null)
                {
                    baselineResults[result.Result] = new List<MatchingResult>() { result };
                }
                else
                {
                    baselineResults[result.Result].Add(result);
                }
            }

            foreach (var result in current)
            {
                if (baselineResults.ContainsKey(result.Result))
                {
                    if (baselineResults[result.Result].Count != 0)
                    {
                        // Pull the first element from the list and match it.  These results are *identical*, so we can just match them in the order they come in.
                        MatchingResult baselineResult = baselineResults[result.Result].First();
                        baselineResults[result.Result].Remove(baselineResult);
                        matchedResults.Add(new MatchedResults() { BaselineResult = baselineResult, CurrentResult = result, MatchingAlgorithm = this });
                    }
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
                if(masked.Properties != null && masked.Properties.ContainsKey(ResultMatchingBaseliner.ResultMatchingResultPropertyName))
                {
                    masked.Properties.Remove(ResultMatchingBaseliner.ResultMatchingResultPropertyName);
                    if(masked.Properties.Count == 0)
                    {
                        masked.Properties = null;
                    }
                }
                return masked;
            }
        }
    }
}
