// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Murmur hash 3 implementation of IHashCalculator. See
    /// http://www.codeproject.com/Articles/32829/Hash-Functions-An-Empirical-Comparison
    /// http://code.google.com/p/smhasher/
    /// http://code.google.com/p/smhasher/source/browse/trunk/MurmurHash3.cpp
    /// </summary>
    /// <remarks>This implementation is the x86_32 version. Note that we don't deal with the "tail" bits of the algorithm
    /// because we allow only integer inputs.</remarks>
    public class MurmurHash
    {
        /// <summary>The first murmur hash constant.</summary>
        private const uint ConstantOne = 0xCC9E2D51u;

        /// <summary>The second murmur hash constant.</summary>
        private const uint ConstantTwo = 0x1B873593u;

        /// <summary>The internal state of the hash.</summary>
        private uint _internalState;

        /// <summary>Number of items inserted into the hash.</summary>
        private uint _itemCount;

        /// <summary>Equality operator.</summary>
        /// <param name="lhs">The left hand side.</param>
        /// <param name="rhs">The right hand side.</param>
        /// <returns>true of <paramref name="lhs"/> is equal to <paramref name="rhs"/>; otherwise, false.</returns>
        public static bool operator ==(MurmurHash lhs, MurmurHash rhs)
        {
            if (object.ReferenceEquals(lhs, null))
            {
                return object.ReferenceEquals(rhs, null);
            }
            else if (object.ReferenceEquals(rhs, null))
            {
                return false;
            }

            return lhs._internalState == rhs._internalState
                && lhs._itemCount == rhs._itemCount;
        }

        /// <summary>Inequality operator.</summary>
        /// <param name="lhs">The left hand side.</param>
        /// <param name="rhs">The right hand side.</param>
        /// <returns>true of <paramref name="lhs"/> is not equal to <paramref name="rhs"/>; otherwise, false.</returns>
        public static bool operator !=(MurmurHash lhs, MurmurHash rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>Adds item to the calculated hash.</summary>
        /// <param name="item">The item to add to the calculated hash.</param>
        public void Add(int item)
        {
            unchecked
            {
                _itemCount++;
                uint istate = _internalState;
                uint uitem = (uint)item;
                uitem *= ConstantOne;
                uitem = RotateLeft(uitem, 15);
                uitem *= ConstantTwo;
                istate ^= uitem;
                istate = RotateLeft(istate, 13);
                istate = (istate * 5) + 0xE6546B64u;
                _internalState = istate;
            }
        }

        /// <summary>Adds multiple items to the calculated hash.</summary>
        /// <param name="items">The items to add.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void AddRange(int[] items)
        {
            foreach (int item in items)
            {
                this.Add(item);
            }
        }

        /// <summary>Adds multiple items to the calculated hash.</summary>
        /// <param name="items">The items to add.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void AddRange(IEnumerable<int> items)
        {
            foreach (int item in items)
            {
                this.Add(item);
            }
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>true if <paramref name="obj" /> and this instance are the same type and
        /// represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as MurmurHash;
            if (other == null)
            {
                return false;
            }
            else
            {
                return other._internalState == _internalState
                    && other._itemCount == _itemCount;
            }
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            uint result = _internalState;
            result ^= _itemCount * 4;
            result = Mix(result);
            return unchecked((int)result);
        }

        /// <summary>Returns a human readable string describing this instance.</summary>
        /// <returns>A human readable string describing this instance.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "MurmurHash state: h: 0x{0:X} count: 0x{1:X}", _internalState, _itemCount);
        }

        /// <summary>Adds item to the calculated hash.</summary>
        /// <param name="item">The item to add to the calculated hash. If this parameter is
        /// <c>null</c>, the effect will be the same as if its
        /// <see cref="System.Object.GetHashCode()" /> function returns 0.</param>
        public void Add(object item)
        {
            if (item == null)
            {
                this.Add(0);
            }
            else
            {
                this.Add(item.GetHashCode());
            }
        }

        /// <summary>Adds a set of items to the calculated hash.</summary>
        /// <param name="items">The items to add. If any item is <c>null</c>, the effect is the
        /// same as if that item's <see cref="System.Object.GetHashCode()" /> returned null.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void AddRange(IEnumerable items)
        {
            foreach (object item in items)
            {
                if (item == null)
                {
                    this.Add(0);
                }
                else
                {
                    this.Add(item.GetHashCode());
                }
            }
        }

        /// <summary>Mixes the given hash value.</summary>
        /// <param name="h">Hash value to process.</param>
        /// <returns>The mixed hash value.</returns>
        /// <remarks>This function must be in an unchecked context.</remarks>
        private static uint Mix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85EBCA6Bu;
            h ^= h >> 13;
            h *= 0xC2B2AE35u;
            h ^= h >> 16;

            return h;
        }

        /// <summary>Rotates left.</summary>
        /// <param name="h">Hash value to process.</param>
        /// <param name="amount">The amount by which <paramref name="h" /> shall be rotated.</param>
        /// <returns>The rotated value of <paramref name="h" />.</returns>
        private static uint RotateLeft(uint h, byte amount)
        {
            return (h << amount) | (h >> (32 - amount));
        }
    }
}
