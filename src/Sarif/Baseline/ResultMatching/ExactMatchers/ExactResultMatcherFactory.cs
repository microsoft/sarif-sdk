// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    public static class ExactResultMatcherFactory
    {
        /// <summary>
        /// Returns a result matcher that matches results that are logically identical (i.e. every part of the result is the same, except for fields used in baselining).
        /// </summary>
        public static IResultMatcher GetIdenticalResultMatcher(bool considerPropertyBagsWhenComparing)
        {
            return new IdenticalResultMatcher(considerPropertyBagsWhenComparing);
        }

        /// <summary>
        /// Returns a result matcher that matches two results when their fingerprints match.
        /// </summary>
        public static IResultMatcher GetFullFingerprintResultMatcher()
        {
            return new FullFingerprintResultMatcher();
        }
    }
}
