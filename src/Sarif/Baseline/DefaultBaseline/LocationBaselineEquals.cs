// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class LocationBaselineEquals : IEqualityComparer<Location>
    {
        public static readonly LocationBaselineEquals Instance = new LocationBaselineEquals();

        public bool Equals(Location x, Location y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                // Target and Result file should match.
                if (!PhysicalLocationBaselineEquals.Instance.Equals(x.AnalysisTarget, y.AnalysisTarget))
                {
                    return false;
                }

                if (!PhysicalLocationBaselineEquals.Instance.Equals(x.ResultFile, y.ResultFile))
                {
                    return false;
                }

                // Code locations (decorated name/fully qualified logical name) should match.
                if (x.DecoratedName != y.DecoratedName || x.FullyQualifiedLogicalName != y.FullyQualifiedLogicalName)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(Location obj)
        {
            int hs = 0;

            hs = hs ^ PhysicalLocationBaselineEquals.Instance.GetHashCode(obj.AnalysisTarget) ^ PhysicalLocationBaselineEquals.Instance.GetHashCode(obj.ResultFile);

            hs = hs ^ obj.DecoratedName.GetNullCheckedHashCode() ^ obj.FullyQualifiedLogicalName.GetNullCheckedHashCode();

            return hs;
        }
    }
}
