// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public static class BitArrayExtensions
    {
        /// <summary>
        ///  Return the number of true values in the BitArray.
        /// </summary>
        /// <param name="array">BitArray to count</param>
        /// <returns>Number of elements set to True</returns>
        public static int TrueCount(this BitArray array)
        {
            int count = 0;

            for (int i = 0; i < array.Count; ++i)
            {
                if (array.Get(i)) { count++; }
            }

            return count;
        }

        /// <summary>
        ///  Filter a List of items to the matching items indicated by a BitArray.
        /// </summary>
        /// <typeparam name="T">Type of items in set</typeparam>
        /// <param name="matches">BitArray identifying which items to include in subset</param>
        /// <param name="set">IList of items to filter</param>
        /// <returns>List of items from set which were included in BitArray</returns>
        public static List<T> MatchingSubset<T>(this BitArray matches, IList<T> set)
        {
            List<T> subset = new List<T>();

            for (int i = 0; i < set.Count; ++i)
            {
                if (matches[i]) { subset.Add(set[i]); }
            }

            return subset;
        }
    }
}
