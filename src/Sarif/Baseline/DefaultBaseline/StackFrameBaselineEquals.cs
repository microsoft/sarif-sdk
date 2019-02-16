// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class StackFrameBaselineEquals : IEqualityComparer<StackFrame>
    {
        public static readonly StackFrameBaselineEquals Instance = new StackFrameBaselineEquals();

        public bool Equals(StackFrame x, StackFrame y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                if (x.Location?.PhysicalLocation?.ArtifactLocation?.Uri != y.Location?.PhysicalLocation?.ArtifactLocation?.Uri)
                {
                    return false;
                }

                if (x.Module != y.Module)
                {
                    return false;
                }

                if (!ListComparisonHelpers.CompareListsOrdered(x.Parameters, y.Parameters))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(StackFrame obj)
        {
            if (ReferenceEquals(obj, null) || obj.Location?.PhysicalLocation?.ArtifactLocation?.Uri == null || obj.Module == null)
            {
                return 0;
            }
            else
            {
                int hs = 0;

                hs = hs ^ obj.Location.PhysicalLocation.ArtifactLocation.Uri.GetNullCheckedHashCode();

                hs = hs ^ obj.Module.GetNullCheckedHashCode();

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsOrdered(obj.Parameters);

                return hs;
            }
        }
    }
}
