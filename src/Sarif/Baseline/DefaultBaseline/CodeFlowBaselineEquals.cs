// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class CodeFlowBaselineEqualityComparator : IEqualityComparer<CodeFlow>
    {
        internal static readonly CodeFlowBaselineEqualityComparator Instance = new CodeFlowBaselineEqualityComparator();

        public bool Equals(CodeFlow x, CodeFlow y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                if (!ListComparisonHelpers.CompareListsOrdered(x.ThreadFlows, y.ThreadFlows, ThreadFlowBaselineEqualityComparator.Instance))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(CodeFlow obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }
            else
            {
                int hs = 0;

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsOrdered(obj.ThreadFlows);

                return hs;
            }
        }
    }
}