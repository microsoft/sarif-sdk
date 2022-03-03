// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Comparers
{
    internal static class ComparerExtensions
    {
        /// Compare 2 object by references.
        /// Return value 'true' presents you can get a definite compare result.
        /// The out parameter 'result' presents compare result:
        /// 0 if both objects are the same or both are null.
        /// -1 if the first object is null and the second object is not null.
        /// 1 if the first object is not null and the second object is null.
        /// Return value 'false' presents the need to compare objects other properties to get the final result.
        public static bool TryReferenceCompares(this object left, object right, out int result)
        {
            result = 0;

            if (object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null)
            {
                result = -1;
                return true;
            }

            if (right == null)
            {
                result = 1;
                return true;
            }

            return false;
        }

        public static int ListCompares<T>(this IList<T> left, IList<T> right) where T : IComparable
        {
            return CompareListHelper(left, right, (a, b) => a.CompareTo(b));
        }

        public static int ListCompares<T>(this IList<T> left, IList<T> right, IComparer<T> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            return CompareListHelper(left, right, comparer.Compare);
        }

        private static int CompareListHelper<T>(IList<T> left, IList<T> right, Func<T, T, int> compareFunction)
        {
            if (compareFunction == null)
            {
                throw new ArgumentNullException(nameof(compareFunction));
            }

            int compareResult = 0;

            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.Count.CompareTo(right.Count);

            if (compareResult != 0)
            {
                return compareResult;
            }

            for (int i = 0; i < left.Count; ++i)
            {
                if (left[i].TryReferenceCompares(right[i], out compareResult) && compareResult != 0)
                {
                    return compareResult;
                }

                compareResult = compareFunction(left[i], right[i]);

                if (compareResult != 0)
                {
                    return compareResult;
                }
            }

            return compareResult;
        }

        public static int DictionaryCompares<T>(this IDictionary<string, T> left, IDictionary<string, T> right) where T : IComparable
        {
            return DictionaryCompareHelper(left, right, (a, b) => a.CompareTo(b));
        }

        public static int DictionaryCompares<T>(this IDictionary<string, T> left, IDictionary<string, T> right, IComparer<T> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            return DictionaryCompareHelper(left, right, comparer.Compare);
        }

        private static int DictionaryCompareHelper<T>(IDictionary<string, T> left, IDictionary<string, T> right, Func<T, T, int> compareFunction)
        {
            if (compareFunction == null)
            {
                throw new ArgumentNullException(nameof(compareFunction));
            }

            int compareResult = 0;

            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.Count.CompareTo(right.Count);

            if (compareResult != 0)
            {
                return compareResult;
            }

            IList<string> leftKeys = left.Keys.ToList();
            IList<string> rightKeys = right.Keys.ToList();

            for (int i = 0; i < leftKeys.Count; ++i)
            {
                compareResult = leftKeys[i].CompareTo(rightKeys[i]);

                if (compareResult != 0)
                {
                    return compareResult;
                }

                compareResult = compareFunction(left[leftKeys[i]], right[rightKeys[i]]);

                if (compareResult != 0)
                {
                    return compareResult;
                }
            }

            return compareResult;
        }

        public static int UriCompares(this Uri left, Uri right)
        {
            int compareResult = 0;

            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            return left.OriginalString.CompareTo(right.OriginalString);
        }
    }
}
