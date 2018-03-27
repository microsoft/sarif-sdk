// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class StackFrameBaselineEquals : IEqualityComparer<StackFrame>
    {
        public static readonly StackFrameBaselineEquals Instance = new StackFrameBaselineEquals();

        public bool Equals(StackFrame x, StackFrame y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                if (x.Uri != y.Uri)
                {
                    return false;
                }
                if (x.FullyQualifiedLogicalName != y.FullyQualifiedLogicalName)
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
            int hs = 0;

            hs = hs ^ obj.Uri.GetNullCheckedHashCode();

            hs = hs ^ obj.FullyQualifiedLogicalName.GetNullCheckedHashCode();

            hs = hs ^ obj.Module.GetNullCheckedHashCode();

            hs = hs ^ ListComparisonHelpers.GetHashOfListContentsOrdered(obj.Parameters);

            return hs;
        }
    }
}
