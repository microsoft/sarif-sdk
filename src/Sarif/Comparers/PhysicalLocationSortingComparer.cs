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
    internal class PhysicalLocationSortingComparer : IComparer<PhysicalLocation>
    {
        internal static readonly PhysicalLocationSortingComparer Instance = new PhysicalLocationSortingComparer();

        public int Compare(PhysicalLocation left, PhysicalLocation right)
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

            compareResult = ArtifactLocationSortingComparer.Instance.Compare(left.ArtifactLocation, right.ArtifactLocation);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = RegionSortingComparer.Instance.Compare(left.Region, right.Region);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = RegionSortingComparer.Instance.Compare(left.ContextRegion, right.ContextRegion);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
