// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>A class that makes atomized string reference comparison easier. Stolen (slightly
    /// modified) from http://referencesource.microsoft.com/#System.Xml/System/Xml/Ref.cs.</summary>
    public static class Ref
    {
        /// <summary>Compares strings for reference equality and asserts if they are equal but not reference equal in debug mode.</summary>
        /// <param name="lhs">The first string to compare.</param>
        /// <param name="rhs">The second string to compare.</param>
        /// <returns>true if <c>Object.ReferenceEquals(lhs, rhs)</c>; otherwise, false.</returns>
        public static bool Equal(string lhs, string rhs)
        {
            Debug.Assert(((object)lhs) == ((object)rhs) || !String.Equals(lhs, rhs, StringComparison.Ordinal),
                "Object comparison used for non-atomized string \"" + lhs + "\".");
            return ((object)lhs) == ((object)rhs);
        }

        /// <summary>Do not call this function. Prevents typo Ref.Equals from compiling.</summary>
        /// <param name="unusedA">Object to be compared.</param>
        /// <param name="unusedB">Object to be compared.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "unusedA")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "unusedB")]
        public static new void Equals(object unusedA, object unusedB)
        { }
    }
}
