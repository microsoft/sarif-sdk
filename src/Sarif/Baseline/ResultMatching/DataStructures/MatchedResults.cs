// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    /// <summary>
    /// 
    /// </summary>
    public class MatchedResults
    {
        public MatchingResult BaselineResult;

        public MatchingResult CurrentResult;

        public IResultMatcher MatchingAlgorithm;
    }
}