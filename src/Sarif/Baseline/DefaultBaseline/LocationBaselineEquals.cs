// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class LocationBaselineEquals : IEqualityComparer<Location>
    {
        public static readonly LocationBaselineEquals Instance = new LocationBaselineEquals();

        public bool Equals(Location x, Location y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                // Result files should match.
                if (!PhysicalLocationBaselineEquals.Instance.Equals(x.PhysicalLocation, y.PhysicalLocation))
                {
                    return false;
                }

                // Code locations (fully qualified logical name) should match.
                if (x.LogicalLocation != null &&
                    y.LogicalLocation != null &&
                    x.LogicalLocation.FullyQualifiedName != y.LogicalLocation.FullyQualifiedName)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(Location obj)
        {
            if (ReferenceEquals(obj, null) || obj.LogicalLocation?.FullyQualifiedName == null)
            {
                return 0;
            }
            else
            {
                int hs = 0;

                hs = hs ^ PhysicalLocationBaselineEquals.Instance.GetHashCode(obj.PhysicalLocation);

                hs = hs ^ obj.LogicalLocation.FullyQualifiedName.GetNullCheckedHashCode();

                return hs;
            }
        }
    }
}
