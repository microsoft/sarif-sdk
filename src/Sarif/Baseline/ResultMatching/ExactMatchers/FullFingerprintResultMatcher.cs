// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    internal class FullFingerprintResultMatcher : IResultMatcher
    {
        public IList<MatchedResults> Match(IList<ExtractedResult> baseline, IList<ExtractedResult> current)
        {
            List<MatchedResults> matchedResults = new List<MatchedResults>();
            Dictionary<Tuple<string, string>, List<ExtractedResult>> baselineResults = new Dictionary<Tuple<string, string>, List<ExtractedResult>>(FingerprintEqualityCalculator.Instance);

            foreach (ExtractedResult result in baseline)
            {
                foreach (string key in result.Result.Fingerprints.Keys)
                {
                    Tuple<string, string> fingerprint = new Tuple<string, string>(key, result.Result.Fingerprints[key]);
                    if (!baselineResults.ContainsKey(fingerprint) || baselineResults[fingerprint] == null)
                    {
                        baselineResults[fingerprint] = new List<ExtractedResult>() { result };
                    }
                    else
                    {
                        baselineResults[fingerprint].Add(result);
                    }
                }
            }

            foreach (ExtractedResult result in current)
            {
                foreach (string key in result.Result.Fingerprints.Keys)
                {
                    Tuple<string, string> fingerprint = new Tuple<string, string>(key, result.Result.Fingerprints[key]);
                    if (baselineResults.ContainsKey(fingerprint) && baselineResults[fingerprint] != null && baselineResults[fingerprint].Count > 0)
                    {
                        ExtractedResult baselineResult = baselineResults[fingerprint].First();
                        baselineResults[fingerprint].Remove(baselineResult);
                        matchedResults.Add(new MatchedResults(baselineResult, result));
                    }
                }
            }

            return matchedResults;
        }

        public class FingerprintEqualityCalculator : IEqualityComparer<Tuple<string, string>>
        {
            public bool Equals(Tuple<string, string> x, Tuple<string, string> y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (x.Item1 == y.Item1 && x.Item2 == y.Item2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode(Tuple<string, string> obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                int hash = -1324097150;

                hash ^= obj.Item1.GetHashCode();
                hash ^= obj.Item2.GetHashCode();

                return hash;
            }

            internal static readonly FingerprintEqualityCalculator Instance = new FingerprintEqualityCalculator();
        }
    }
}
