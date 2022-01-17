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
    internal class LocationSortingComparer : IComparer<Location>
    {
        internal static readonly LocationSortingComparer Instance = new LocationSortingComparer();

        public int Compare(Location left, Location right)
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
            compareResult = left.Id.CompareTo(right.Id);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = PhysicalLocationSortingComparer.Instance.Compare(left.PhysicalLocation, right.PhysicalLocation);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.LogicalLocations, right.LogicalLocations))
            {
                if (left.LogicalLocations == null)
                {
                    return -1;
                }

                if (right.LogicalLocations == null)
                {
                    return 1;
                }

                compareResult = left.LogicalLocations.Count.CompareTo(right.LogicalLocations.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.LogicalLocations.Count; ++i)
                {
                    compareResult = LogicalLocationSortingComparer.Instance.Compare(left.LogicalLocations[i], right.LogicalLocations[i]);
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
