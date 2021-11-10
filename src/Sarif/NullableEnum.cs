// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A wrapper class for Nullable Enum
    /// </summary>
    public static class NullableEnum
    {
        /// <summary>
        /// Parse a string into Nullable Enum.
        /// </summary>
        /// <param name="value">
        /// The string value to parse.
        /// </param>
        /// <returns>
        /// The Nullable Enum matching the value.
        /// </returns>
        public static T Parse<T>(string value)
        {
            Type enumType = Nullable.GetUnderlyingType(typeof(T));
            return Enum.IsDefined(enumType, value)
                ? (T)Enum.Parse(enumType, value)
                : default;
        }

        /// <summary>
        /// Parse a string array into Nullable Enum array.
        /// </summary>
        /// <param name="values">
        /// The array of string value to parse.
        /// </param>
        /// <returns>
        /// The Nullable Enum array matching the value array.
        /// </returns>
        public static IEnumerable<T> Parse<T>(string[] values)
        {
            if (values == null)
            {
                return null;
            }

            var list = new List<T>(values.Length);

            foreach (string item in values)
            {
                list.Add(Parse<T>(item));
            }

            return list;
        }
    }
}
