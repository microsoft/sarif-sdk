// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace SarifBaseline.Extensions
{
    /// <summary>
    ///  DirectResultMatchingComparer is an adapter for ResultMatchingComparer for bare Results.
    ///  This sorts Results as needed for the Result Matching algorithm.
    ///  This adapter is used to keep persisted baselines ideally sorted.
    /// </summary>
    public class DirectResultMatchingComparer : IComparer<Result>
    {
        public static DirectResultMatchingComparer Instance = new DirectResultMatchingComparer();

        public int Compare(Result left, Result right)
        {
            return ResultMatchingComparer.Instance.Compare(new ExtractedResult(left, left.Run), new ExtractedResult(right, right.Run));
        }
    }
}
