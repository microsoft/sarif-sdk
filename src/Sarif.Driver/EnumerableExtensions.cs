// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>A class containing common useful extension methods for <see cref="System.Collections.Generic.IEnumerable{T}"/>.</summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Compares two sequences of elements lexicographically, using the element type's default
        /// comparer.
        /// </summary>
        /// <typeparam name="T">The type of element in the sequences being compared.</typeparam>
        /// <param name="leftSequence">The left hand side sequence to lexicographically compare.</param>
        /// <param name="rightSequence">The right hand side sequence to lexicographically compare.</param>
        /// <returns>
        /// If the range <paramref name="leftSequence"/> is lexicographically less than the range
        /// <paramref name="rightSequence"/>, a negative value. Otherwise, if the range
        /// <paramref name="rightSequence"/> is lexicographically less than the range
        /// <paramref name="leftSequence"/>, a positive value. Otherwise, (the sequences are equal) 0.
        /// </returns>
        public static int LexicographicalCompare<T>(this IEnumerable<T> leftSequence, IEnumerable<T> rightSequence)
        {
            return leftSequence.LexicographicalCompare(rightSequence, Comparer<T>.Default);
        }

        /// <summary>
        /// Compares two sequences of elements lexicographically, using the supplied element comparer.
        /// </summary>
        /// <typeparam name="T">The type of element in the sequences being compared.</typeparam>
        /// <param name="leftSequence">The left hand side sequence to lexicographically compare.</param>
        /// <param name="rightSequence">The right hand side sequence to lexicographically compare.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>
        /// If the range <paramref name="leftSequence"/> is lexicographically less than the range
        /// <paramref name="rightSequence"/>, a negative value. Otherwise, if the range
        /// <paramref name="rightSequence"/> is lexicographically less than the range
        /// <paramref name="leftSequence"/>, a positive value. Otherwise, (the sequences are equal) 0.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public static int LexicographicalCompare<T>(this IEnumerable<T> leftSequence, IEnumerable<T> rightSequence, IComparer<T> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }

            using (IEnumerator<T> leftEnumerator = leftSequence.GetEnumerator())
            using (IEnumerator<T> rightEnumerator = rightSequence.GetEnumerator())
                for (;;)
                {
                    bool leftHasElements = leftEnumerator.MoveNext();
                    bool rightHasElements = rightEnumerator.MoveNext();

                    if (leftHasElements && rightHasElements)
                    {
                        int elementComparisonResult = comparer.Compare(leftEnumerator.Current, rightEnumerator.Current);
                        if (elementComparisonResult != 0)
                        {
                            return elementComparisonResult;
                        }
                    }
                    else if (!leftHasElements && !rightHasElements)
                    {
                        // Sequences are equal.
                        return 0;
                    }
                    else if (rightHasElements)
                    {
                        // Left sequence is a prefix of the right sequence; so left < right.
                        return -1;
                    }
                    else /* if (leftHasElements) */
                    {
                        // Right sequence is a prefix of the left sequence, so right < left.
                        return 1;
                    }
                }
        }

        /// <summary>Removes elements from an array if they match a predicate.</summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to search for elements matching the specified predicate. If this parameter is <c>null</c>, no action is taken and false is returned.</param>
        /// <param name="predicate">The predicate to check against.</param>
        /// <returns>true if any elements were removed from <paramref name="array"/>; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")] // Required to match the semantics of Array.Resize
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")] // FxCop noise; we are checking that `array` isn't null
        public static bool RemoveIf<T>(ref T[] array, Func<T, bool> predicate)
        {
            if (array == null)
            {
                return false;
            }

            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            int newLogicalEnd = 0;
            int last = array.Length;
            for (int first = 0; first != last; ++first)
            {
                if (!predicate(array[first]))
                {
                    array[newLogicalEnd++] = array[first];
                }
            }

            if (newLogicalEnd == last)
            {
                return false;
            }
            else
            {
                Array.Resize(ref array, newLogicalEnd);
                return true;
            }
        }

        /// <summary>
        /// Generates an enumerable collection in a random order.
        /// </summary>
        /// <typeparam name="T">The type stored in the enumerable collection.</typeparam>
        /// <param name="sequence">The enumerable containing the original data to scramble.</param>
        /// <returns>The sequence <paramref name="sequence"/> in a scrambled order.</returns>
        public static IList<T> Shuffle<T>(this IEnumerable<T> sequence)
        {
            return sequence.Shuffle(new Random());
        }

        /// <summary>
        /// Generates an enumerable collection in a random order.
        /// </summary>
        /// <typeparam name="T">The type stored in the enumerable collection.</typeparam>
        /// <param name="sequence">The enumerable containing the original data to scramble.</param>
        /// <param name="randomNumberGenerator">The random number generator to use.</param>
        /// <returns>The sequence <paramref name="sequence"/> in a scrambled order.</returns>
        public static IList<T> Shuffle<T>(this IEnumerable<T> sequence, Random randomNumberGenerator)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException("sequence");
            }

            if (randomNumberGenerator == null)
            {
                throw new ArgumentNullException("randomNumberGenerator");
            }

            // A naïve Knuth Fischer Yates shuffle.
            T swapTemp;
            List<T> values = sequence.ToList();
            int currentlySelecting = values.Count;
            while (currentlySelecting > 1)
            {
                // Next returns the next integer [0, currentlySelecting) which is why we need
                // to get the selected element before decrementing currentlySelecting
                // (To make it possible that the currentlySelecting is swapped with itself)
                int selectedElement = randomNumberGenerator.Next(currentlySelecting);
                --currentlySelecting;
                if (currentlySelecting != selectedElement)
                {
                    swapTemp = values[currentlySelecting];
                    values[currentlySelecting] = values[selectedElement];
                    values[selectedElement] = swapTemp;
                }
            }

            return values;
        }
    }
}
