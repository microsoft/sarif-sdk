// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class ThreadFlowBaselineEqualityComparator : IEqualityComparer<ThreadFlow>
    {
        internal static readonly ThreadFlowBaselineEqualityComparator Instance = new ThreadFlowBaselineEqualityComparator();

        public bool Equals(ThreadFlow x, ThreadFlow y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                if (!ListComparisonHelpers.CompareListsOrdered(x.Locations, y.Locations, ThreadFlowLocationBaselineEquals.DefaultInstance))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(ThreadFlow obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }
            else
            {
                int hs = 0;

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsOrdered(obj.Locations);

                return hs;
            }
        }
    }
}