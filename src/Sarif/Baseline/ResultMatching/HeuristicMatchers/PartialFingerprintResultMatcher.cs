// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    /// <summary>
    /// Compares two results, and declares them equal if all of their partial fingerprints match.
    /// 
    /// TODO:  Handle versioning of partial fingerprints.
    /// </summary>
    internal class PartialFingerprintResultMatcher : HeuristicMatcher
    {
        public PartialFingerprintResultMatcher() : base(PartialFingerprintResultComparer.Instance) { }

        public class PartialFingerprintResultComparer : IResultMatchingComparer
        {
            public static readonly PartialFingerprintResultComparer Instance = new PartialFingerprintResultComparer();

            public bool Equals(ExtractedResult x, ExtractedResult y)
            {
                return CompareDictionaries(x.Result.PartialFingerprints, y.Result.PartialFingerprints);
            }

            private bool CompareDictionaries(IDictionary<string, string> x, IDictionary<string, string> y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (x.Keys.Count != y.Keys.Count)
                {
                    return false;
                }

                foreach (string key in x.Keys)
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

            public int GetHashCode(ExtractedResult obj)
            {
                if (obj == null || obj.Result == null || obj.Result.PartialFingerprints == null || !obj.Result.PartialFingerprints.Any())
                {
                    return 0;
                }

                int hash = -1324097150;

                foreach (string key in obj.Result.PartialFingerprints.Keys)
                {
                    int keyHash = key.GetHashCode();
                    int resultHash = obj.Result.PartialFingerprints[key].GetHashCode();

                    // hash = current hash XOR hash of the key rotated by 16 bits XOR the hash of the result
                    hash ^= (keyHash << 16 | keyHash >> (32 - 16)) ^ resultHash;
                }

                return hash;
            }

            public bool ResultMatcherApplies(ExtractedResult result)
            {
                return (result.Result.PartialFingerprints != null && result.Result.PartialFingerprints.Any());
            }
        }
    }
}
