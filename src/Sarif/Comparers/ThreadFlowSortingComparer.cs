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
    internal class ThreadFlowSortingComparer : IComparer<ThreadFlow>
    {
        internal static readonly ThreadFlowSortingComparer Instance = new ThreadFlowSortingComparer();

        public int Compare(ThreadFlow left, ThreadFlow right)
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
            compareResult = string.Compare(left.Id, right.Id);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = MessageSortingComparer.Instance.Compare(left.Message, right.Message);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.Locations, right.Locations))
            {
                if (left.Locations == null)
                {
                    return -1;
                }

                if (right.Locations == null)
                {
                    return 1;
                }

                compareResult = left.Locations.Count.CompareTo(right.Locations.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.Locations.Count; ++i)
                {
                    compareResult = ThreadFlowLocationSortingComparer.Instance.Compare(left.Locations[i], right.Locations[i]);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            return compareResult;
        }
    }
}
