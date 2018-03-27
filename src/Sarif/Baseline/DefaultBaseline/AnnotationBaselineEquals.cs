// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class AnnotationBaselineEquals : IEqualityComparer<Annotation>
    {
        internal static readonly AnnotationBaselineEquals Instance = new AnnotationBaselineEquals();

        public bool Equals(Annotation x, Annotation y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                if (!ListComparisonHelpers.CompareListsAsSets(x.Locations, y.Locations, PhysicalLocationBaselineEquals.Instance))
                {
                    return false;
                }

                if (x.Message != y.Message)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(Annotation obj)
        {
            int hs = 0;

            hs = hs ^ obj.Message.GetNullCheckedHashCode();

            hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.Locations, PhysicalLocationBaselineEquals.Instance);

            return hs;
        }
    }
}