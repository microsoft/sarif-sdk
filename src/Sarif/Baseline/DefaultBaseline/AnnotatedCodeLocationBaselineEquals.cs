// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class AnnotatedCodeLocationBaselineEquals : IEqualityComparer<AnnotatedCodeLocation>
    {
        internal static readonly AnnotatedCodeLocationBaselineEquals DefaultInstance = new AnnotatedCodeLocationBaselineEquals();
        
        public bool Equals(AnnotatedCodeLocation x, AnnotatedCodeLocation y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                if (x.FullyQualifiedLogicalName != y.FullyQualifiedLogicalName || x.LogicalLocationKey != y.LogicalLocationKey)
                {
                    return false;
                }

                if (x.Importance != y.Importance)
                {
                    return false;
                }

                if (x.Module != y.Module)
                {
                    return false;
                }

                if (x.Kind != y.Kind)
                {
                    return false;
                }

                if (x.Target != y.Target || x.TargetKey != y.TargetKey)
                {
                    return false;
                }

                if (!PhysicalLocationBaselineEquals.Instance.Equals(x.PhysicalLocation, y.PhysicalLocation))
                {
                    return false;
                }

                if (!ListComparisonHelpers.CompareListsAsSets(x.Annotations, y.Annotations, AnnotationBaselineEquals.Instance))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(AnnotatedCodeLocation obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }
            else
            {
                int hs = 0;

                hs = hs ^ obj.FullyQualifiedLogicalName.GetNullCheckedHashCode() ^ obj.Importance.GetNullCheckedHashCode() ^ obj.Kind.GetNullCheckedHashCode() ^ obj.Module.GetNullCheckedHashCode();

                hs = hs ^ obj.Target.GetNullCheckedHashCode() ^ obj.TargetKey.GetNullCheckedHashCode() ^ obj.LogicalLocationKey.GetNullCheckedHashCode();

                hs = hs ^ PhysicalLocationBaselineEquals.Instance.GetHashCode(obj.PhysicalLocation);

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.Annotations, AnnotationBaselineEquals.Instance);

                return hs;
            }
        }
    }
}