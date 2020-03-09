// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal static class ListComparisonHelpers
    {
        // Use the default or a given equality comparator to determine if two lists of elements are equal (with order mattering)
        internal static bool CompareListsOrdered<T>(IList<T> left, IList<T> right, IEqualityComparer<T> equalityComparer = null)
        {
            if (!object.ReferenceEquals(left, right))
            {
                if (left == null || right == null)
                {
                    return false;
                }

                if (left.Count != right.Count)
                {
                    return false;
                }

                for (int i = 0; i < left.Count; i++)
                {
                    if (equalityComparer != null && equalityComparer.Equals(left[i], right[i]))
                    {
                        return false;
                    }
                    else if (equalityComparer == null && left[i].Equals(right[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Use the default or a given equality comparator to determine if two lists of elements are equal, ignoring order.
        internal static bool CompareListsAsSets<T>(IList<T> left, IList<T> right, IEqualityComparer<T> equalityComparer = null)
        {
            if (!object.ReferenceEquals(left, right))
            {
                if (left == null || right == null)
                {
                    return false;
                }

                if (left.Count != right.Count)
                {
                    return false;
                }

                foreach (T l in left)
                {
                    if (equalityComparer != null && !right.Any(r => equalityComparer.Equals(l, r)))
                    {
                        return false;
                    }
                    else if (equalityComparer == null && !right.Any(r => r.Equals(l)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static int GetHashOfListContentsAsSets<T>(IList<T> obj, IEqualityComparer<T> equalityComparer = null)
        {
            int hs = 0;
            if (obj != null)
            {
                for (int i = 0; i < obj.Count; i++)
                {
                    if (equalityComparer == null)
                    {
                        hs = hs ^ obj[i].GetHashCode();
                    }
                    else
                    {
                        hs = hs ^ equalityComparer.GetHashCode(obj[i]);
                    }
                }
            }
            return hs;
        }

        internal static int GetHashOfListContentsOrdered<T>(IList<T> obj, IEqualityComparer<T> equalityComparer = null)
        {
            uint hs = 0;
            if (obj != null)
            {
                for (int i = 0; i < obj.Count; i++)
                {
                    hs = (hs << 1) | (hs >> 31); // Rotate by 1 bit.
                    if (equalityComparer == null)
                    {
                        hs = hs ^ (uint)obj[i].GetHashCode();
                    }
                    else
                    {
                        hs = hs ^ (uint)equalityComparer.GetHashCode(obj[i]);
                    }
                }
            }
            return (int)hs;
        }
    }
}
