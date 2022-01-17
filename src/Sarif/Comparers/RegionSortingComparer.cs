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
    internal class RegionSortingComparer : IComparer<Region>
    {
        internal static readonly RegionSortingComparer Instance = new RegionSortingComparer();

        public int Compare(Region left, Region right)
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
            compareResult = left.StartLine.CompareTo(right.StartLine);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.StartColumn.CompareTo(right.StartColumn);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.EndLine.CompareTo(right.EndLine);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.EndColumn.CompareTo(right.EndColumn);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.CharOffset.CompareTo(right.CharOffset);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.CharLength.CompareTo(right.CharLength);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ByteOffset.CompareTo(right.ByteOffset);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ByteLength.CompareTo(right.ByteLength);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
