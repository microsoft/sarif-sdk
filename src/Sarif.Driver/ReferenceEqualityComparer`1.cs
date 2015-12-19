// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Reference equals equality comparer.</summary>
    /// <typeparam name="T">Generic type compared.</typeparam>
    /// <seealso cref="System.Collections.Generic.IEqualityComparer&lt;T&gt;"/>
    public class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        /// <summary>The instance of equality comparer for T.</summary> 
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

        /// <summary>
        /// Prevents clients from constructing <see cref="ReferenceEqualityComparer{T}"/> instances.
        /// </summary>
        private ReferenceEqualityComparer()
        { }

        /// <summary>Tests if two T objects are considered equal.</summary>
        /// <param name="x">Left T to be compared.</param>
        /// <param name="y">Right T to be compared.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        /// <seealso cref="System.Collections.Generic.IEqualityComparer&lt;T&gt;.Equals(T,T)"/>
        public bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        /// <summary>Calculates the hash code for a given T.</summary>
        /// <param name="obj">The object to get a hash code for.</param>
        /// <returns>The hash code for <paramref name="obj"/>.</returns>
        /// <seealso cref="System.Collections.Generic.IEqualityComparer&lt;T&gt;.GetHashCode(T)"/>
        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
