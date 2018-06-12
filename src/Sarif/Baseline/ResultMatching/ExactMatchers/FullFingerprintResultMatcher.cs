// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    // TODO: Before adding this to the baseliner, this needs to not match on results w/o fingerprints, and this needs to match fingerprints if *any* of the entries match, not if *all* of the entries match.
    internal class FullFingerprintResultMatcher : IResultMatcher
    {
        public IEnumerable<MatchedResults> MatchResults(IEnumerable<MatchingResult> baseline, IEnumerable<MatchingResult> current)
        {
            List<MatchedResults> matchedResults = new List<MatchedResults>();
            Dictionary<IDictionary<string, string>, MatchingResult> baselineResults = new Dictionary<IDictionary<string, string>, MatchingResult>(FingerprintEqualityCalculator.Instance);

            foreach (var result in baseline)
            {
                baselineResults.Add(result.Result.Fingerprints, result);
            }

            foreach (var result in current)
            {
                if (baselineResults.ContainsKey(result.Result.Fingerprints) && baselineResults[result.Result.Fingerprints] != null)
                {
                    matchedResults.Add(new MatchedResults() { BaselineResult = baselineResults[result.Result.Fingerprints], CurrentResult = result, MatchingAlgorithm = this });
                }
            }

            return matchedResults;
        }

        public class FingerprintEqualityCalculator : IEqualityComparer<IDictionary<string, string>>
        {
            public bool Equals(IDictionary<string, string> x, IDictionary<string, string> y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if(x.Keys.Count != y.Keys.Count)
                {
                    return false;
                }

                foreach (var key in x.Keys)
                {
                    if (!y.ContainsKey(key))
                    {
                        return false;
                    }
                    if (y[key] != x[key])
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(IDictionary<string, string> obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                int hash = -1324097150;

                foreach (var key in obj.Keys)
                {
                    int keyHash = key.GetHashCode();
                    int resultHash = obj[key].GetHashCode();

                    // hash = current hash XOR hash of the key rotated by 16 bits XOR the hash of the result
                    hash ^= (keyHash << 16 | keyHash >> (32-16))^ resultHash;
                }

                return hash;
            }

            internal static readonly FingerprintEqualityCalculator Instance = new FingerprintEqualityCalculator();
        }

    }
}
