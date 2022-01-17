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
    internal class LogicalLocationSortingComparer : IComparer<LogicalLocation>
    {
        internal static readonly LogicalLocationSortingComparer Instance = new LogicalLocationSortingComparer();

        public int Compare(LogicalLocation left, LogicalLocation right)
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
            compareResult = string.Compare(left.Name, right.Name);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Index.CompareTo(right.Index);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.FullyQualifiedName, right.FullyQualifiedName);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.DecoratedName, right.DecoratedName);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ParentIndex.CompareTo(right.ParentIndex);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Kind, right.Kind);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
