// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class StackBaselineEquals : IEqualityComparer<Stack>
    {
        public static readonly StackBaselineEquals Instance = new StackBaselineEquals();

        public bool Equals(Stack x, Stack y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                if (!ListComparisonHelpers.CompareListsOrdered(x.Frames, y.Frames))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(Stack obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }
            else
            {
                int hs = 0;

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsOrdered(obj.Frames, StackFrameBaselineEquals.Instance);

                return hs;
            }
        }
    }
}
