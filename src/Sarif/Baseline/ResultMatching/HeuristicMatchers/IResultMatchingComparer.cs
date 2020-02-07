// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    interface IResultMatchingComparer : IEqualityComparer<ExtractedResult>
    {
        /// <summary>
        /// Checks if the result matcher applies to a particular result.
        /// For example, we don't want to try to match results without partial fingerprints in a partial fingerprint matcher.
        /// </summary>
        /// <param name="result">Matching result to check.</param>
        /// <returns>True if the result matcher applies, false if it does not.</returns>
        bool ResultMatcherApplies(ExtractedResult result);
    }
}
