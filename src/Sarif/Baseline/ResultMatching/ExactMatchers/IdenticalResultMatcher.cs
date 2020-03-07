// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    /// <summary>
    /// Matches two results if every part of the result is identical, except the fields used in baselining.
    /// 
    /// If a run contains multiple identical results, we will match them in order.
    /// </summary>
    internal class IdenticalResultMatcher : IResultMatcher
    {
        private readonly bool _considerPropertyBagsWhenComparing;

        public IdenticalResultMatcher(bool considerPropertyBagsWhenComparing)
        {
            _considerPropertyBagsWhenComparing = considerPropertyBagsWhenComparing;
        }

        public IList<MatchedResults> Match(IList<ExtractedResult> baseline, IList<ExtractedResult> current)
        {
            List<MatchedResults> matchedResults = new List<MatchedResults>();

            IdenticalResultEqualityComparer comparer = _considerPropertyBagsWhenComparing
                ? IdenticalResultEqualityComparer.PropertyBagComparingInstance
                : IdenticalResultEqualityComparer.PropertyBagIgnoringInstance;

            Dictionary<Result, List<ExtractedResult>> baselineResults = new Dictionary<Result, List<ExtractedResult>>(comparer);

            foreach (ExtractedResult result in baseline)
            {
                if (!baselineResults.ContainsKey(result.Result) || baselineResults[result.Result] == null)
                {
                    baselineResults[result.Result] = new List<ExtractedResult>() { result };
                }
                else
                {
                    baselineResults[result.Result].Add(result);
                }
            }

            foreach (ExtractedResult result in current)
            {
                if (baselineResults.ContainsKey(result.Result))
                {
                    if (baselineResults[result.Result].Count != 0)
                    {
                        // Pull the first element from the list and match it.  These results are *identical*, so we can just match them in the order they come in.
                        ExtractedResult baselineResult = baselineResults[result.Result].First();
                        baselineResults[result.Result].Remove(baselineResult);
                        matchedResults.Add(new MatchedResults(baselineResult, result));
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
            private readonly bool _considerPropertyBagsWhenComparing;

            public IdenticalResultEqualityComparer(bool considerPropertyBagsWhenComparing)
            {
                _considerPropertyBagsWhenComparing = considerPropertyBagsWhenComparing;
            }

            public static readonly IdenticalResultEqualityComparer PropertyBagComparingInstance = new IdenticalResultEqualityComparer(considerPropertyBagsWhenComparing: true);

            public static readonly IdenticalResultEqualityComparer PropertyBagIgnoringInstance = new IdenticalResultEqualityComparer(considerPropertyBagsWhenComparing: false);


            public bool Equals(Result x, Result y)
            {
                return ResultEqualityComparer.Instance.Equals(CreateMaskedResult(x), CreateMaskedResult(y));
            }

            public int GetHashCode(Result obj)
            {
                return ResultEqualityComparer.Instance.GetHashCode(CreateMaskedResult(obj));
            }

            /// <summary>
            /// We create a deep copy of the result (so as to not change the actual result contents), and mask the
            /// fields that relate to result matching (status, suppression states, correllation GUID, etc.).
            /// </summary>
            /// <param name="result">The original result</param>
            /// <returns>A masked deep copy of the result.</returns>
            public Result CreateMaskedResult(Result result)
            {
                Result masked = result.DeepClone();
                masked.CorrelationGuid = null;
                masked.Suppressions = null;
                masked.BaselineState = BaselineState.None;

                if (_considerPropertyBagsWhenComparing)
                {
                    if (masked.Properties != null && masked.Properties.ContainsKey(SarifLogResultMatcher.ResultMatchingResultPropertyName))
                    {
                        masked.Properties.Remove(SarifLogResultMatcher.ResultMatchingResultPropertyName);
                        if (masked.Properties.Count == 0)
                        {
                            masked.Properties = null;
                        }
                    }
                }
                else
                {
                    masked.Properties?.Clear();
                }

                return masked;
            }
        }
    }
}
