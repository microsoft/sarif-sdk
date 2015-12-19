// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Multiply by primes hash calculator.</summary>
    public class MultiplyByPrimesHash
    {
        /// <summary>Internal state of the hash.</summary>
        private int _state;

        /// <summary>Initializes a new instance of the MultiplyByPrimesHash class.</summary>
        public MultiplyByPrimesHash()
        {
            _state = 17;
        }

        /// <summary>Equality operator.</summary>
        /// <param name="lhs">The left hand side.</param>
        /// <param name="rhs">The right hand side.</param>
        /// <returns>true of <paramref name="lhs"/> is equal to <paramref name="rhs"/>; otherwise, false.</returns>
        public static bool operator ==(MultiplyByPrimesHash lhs, MultiplyByPrimesHash rhs)
        {
            if (object.ReferenceEquals(lhs, null))
            {
                return object.ReferenceEquals(rhs, null);
            }
            else if (object.ReferenceEquals(rhs, null))
            {
                return false;
            }

            return lhs._state == rhs._state;
        }

        /// <summary>Inequality operator.</summary>
        /// <param name="lhs">The left hand side.</param>
        /// <param name="rhs">The right hand side.</param>
        /// <returns>true of <paramref name="lhs"/> is not equal to <paramref name="rhs"/>; otherwise, false.</returns>
        public static bool operator !=(MultiplyByPrimesHash lhs, MultiplyByPrimesHash rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>Adds item to the calculated hash.</summary>
        /// <param name="item">The item to add to the calculated hash.</param>
        public void Add(int item)
        {
            _state = unchecked((_state * 31) + item);
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
            var other = obj as MultiplyByPrimesHash;
            if (other == null)
            {
                return false;
            }
            else
            {
                return other._state == _state;
            }
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return _state;
        }

        /// <summary>Returns a human readable string describing this instance.</summary>
        /// <returns>A human readable string describing this instance.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "MultiplyByPrimesHash state: 0x{0:X}", _state);
        }

        /// <summary>Adds item to the calculated hash.</summary>
        /// <param name="item">The item to add to the calculated hash. If this parameter is
        /// <c>null</c>, the effect will be the same as if its
        /// <see cref="System.Object.GetHashCode()" /> function returns 0.</param>
        public void Add(object item)
        {
            this.Add(item == null ? 0 : item.GetHashCode());
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
    }
}
