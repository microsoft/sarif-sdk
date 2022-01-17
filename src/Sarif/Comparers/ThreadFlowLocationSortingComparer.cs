// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

/// <summary>
/// All Comparer implementations should be replaced by auto-generated codes by JSchema as 
/// part of EqualityComparer in a planned comprehensive solution.
/// Tracking by issue: https://github.com/microsoft/jschema/issues/141
/// </summary>
namespace Microsoft.CodeAnalysis.Sarif
{
    internal class ThreadFlowLocationSortingComparer : IComparer<ThreadFlowLocation>
    {
        internal static readonly ThreadFlowLocationSortingComparer Instance = new ThreadFlowLocationSortingComparer();

        public int Compare(ThreadFlowLocation left, ThreadFlowLocation right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            int compareResult = 0;
            compareResult = left.Index.CompareTo(right.Index);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = LocationSortingComparer.Instance.Compare(left.Location, right.Location);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.Kinds, right.Kinds))
            {
                if (left.Kinds == null)
                {
                    return -1;
                }

                if (right.Kinds == null)
                {
                    return 1;
                }

                compareResult = left.Kinds.Count.CompareTo(right.Kinds.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.Kinds.Count; ++i)
                {
                    compareResult = string.Compare(left.Kinds[i], right.Kinds[i]);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            compareResult = left.NestingLevel.CompareTo(right.NestingLevel);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ExecutionOrder.CompareTo(right.ExecutionOrder);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ExecutionTimeUtc.CompareTo(right.ExecutionTimeUtc);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Importance.CompareTo(right.Importance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
