// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    public static class HeuristicResultMatcherFactory
    {
        public static IResultMatcher GetContextRegionHeuristicResultMatcher()
        {
            return new ContextRegionHeuristicMatcher();
        }

        public static IResultMatcher GetPartialFingerprintResultMatcher()
        {
            return new PartialFingerprintResultMatcher();
        }
    }
}
