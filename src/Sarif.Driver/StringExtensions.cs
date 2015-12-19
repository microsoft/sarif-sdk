// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Extensions on the <see cref="System.String"/> class.</summary>
    public static class StringExtensions
    {
        /// <summary>Gets a null value for null or whitespace strings. Otherwise passes though the source string unchanged.</summary>
        /// <param name="target">The string to check.</param>
        /// <returns>If <paramref name="target"/> is <c>null</c> or whitespace, <c>null</c>; otherwise <paramref name="target"/>.</returns>
        public static string ClampToNullIfWhiteSpace(this string target)
        {
            if (String.IsNullOrWhiteSpace(target))
            {
                return null;
            }

            return target;
        }
    }
}
