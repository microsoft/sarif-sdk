// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Comparers
{
    internal class ComparerHelper
    {
        /// <summary>
        /// Compare 2 object refences. Return value false presents need to
        /// compare objects values to get final result.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <param name="result">
        /// 0 if both objects are same or both are null.
        /// -1 if the first object is null and the second object is not null.
        /// 1 if the first object is not null and the second object is null.
        /// </param>
        /// <returns>Return true if can get a definite compare result, otherwise return false.</returns>
        public static bool CompareReference(object left, object right, out int result)
        {
            result = 0;

            if (object.ReferenceEquals(left, right))
            {
                result = 0;
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

        /// <summary>
        /// Compare 2 lists of type T derived from IComparable.
        /// </summary>
        /// <typeparam name="T">type derived from IComparable</typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static int CompareList<T>(IList<T> left, IList<T> right) where T : IComparable
        {
            return CompareListHelper(left, right, (a, b) => a.CompareTo(b));
        }

        /// <summary>
        /// Compare 2 lists of type T using a Comparer<T>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static int CompareList<T>(IList<T> left, IList<T> right, IComparer<T> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            return CompareListHelper(left, right, comparer.Compare);
        }

        private static int CompareListHelper<T>(IList<T> left, IList<T> right, Func<T, T, int> compareFunc)
        {
            if (compareFunc == null)
            {
                throw new ArgumentNullException(nameof(compareFunc));
            }

            if (CompareReference(left, right, out int compareResult))
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
                if (CompareReference(left[i], right[i], out compareResult) && compareResult != 0)
                {
                    return compareResult;
                }

                compareResult = compareFunc(left[i], right[i]);

                if (compareResult != 0)
                {
                    return compareResult;
                }
            }

            return compareResult;
        }

        public static int CompareDictionary<T>(IDictionary<string, T> left, IDictionary<string, T> right) where T : IComparable
        {
            return CompareDictionaryHelper(left, right, (a, b) => a.CompareTo(b));
        }

        public static int CompareDictionary<T>(IDictionary<string, T> left, IDictionary<string, T> right, IComparer<T> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            return CompareDictionaryHelper(left, right, comparer.Compare);
        }

        private static int CompareDictionaryHelper<T>(IDictionary<string, T> left, IDictionary<string, T> right, Func<T, T, int> compareFunc)
        {
            if (compareFunc == null)
            {
                throw new ArgumentNullException(nameof(compareFunc));
            }

            if (CompareReference(left, right, out int compareResult))
            {
                return compareResult;
            }

            compareResult = left.Count.CompareTo(right.Count);

            if (compareResult != 0)
            {
                return compareResult;
            }

            IList<string> leftKeys = left.Keys.OrderBy(k => k).ToList();
            IList<string> rightKeys = right.Keys.OrderBy(k => k).ToList();

            for (int i = 0; i < leftKeys.Count; ++i)
            {
                compareResult = leftKeys[i].CompareTo(rightKeys[i]);

                if (compareResult != 0)
                {
                    return compareResult;
                }

                compareResult = compareFunc(left[leftKeys[i]], right[rightKeys[i]]);

                if (compareResult != 0)
                {
                    return compareResult;
                }
            }

            return compareResult;
        }

        /// <summary>
        /// Compare 2 Uri objects using same parameters.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static int CompareUri(Uri left, Uri right)
        {
            if (CompareReference(left, right, out int compareResult))
            {
                return compareResult;
            }

            return left.OriginalString.CompareTo(right.OriginalString);
        }
    }
}
