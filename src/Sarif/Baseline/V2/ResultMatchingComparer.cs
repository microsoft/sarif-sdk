// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    /// <summary>
    ///  ResultMatchingComparer sorts Results for the Result Matching algorithm.
    /// </summary>
    public class ResultMatchingComparer : IComparer<ExtractedResult>
    {
        public static ResultMatchingComparer Instance = new ResultMatchingComparer();

        public int Compare(ExtractedResult left, ExtractedResult right)
        {
            // Compare by Where first
            int whereCompare = WhereComparer.Instance.Compare(left, right);
            if (whereCompare != 0) { return whereCompare; }

            // If tied, compare by RuleId
            // If multiple rules match a whole line, this ensures the right matches are compared to each other
            int ruleCompare = left.Result.GetRule(left.OriginalRun).Id.CompareTo(right.Result.GetRule(right.OriginalRun).Id);
            return ruleCompare;
        }
    }
}
