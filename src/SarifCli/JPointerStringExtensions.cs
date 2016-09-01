// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Cli
{
    // TODO: These methods belong in the Microsoft.Json.Pointer library.
    // Filed https://github.com/Microsoft/sarif-sdk/issues/512 for this.
    internal static class JPointerStringExtensions
    {
        internal static string AtProperty(this string jPointer, string propertyName)
        {
            return $"{jPointer}/{propertyName.EscapeJsonPointer()}";
        }

        internal static string AtIndex(this string jPointer, int index)
        {
            return $"{jPointer}/{index}";
        }

        // The components of a JSON Pointer are separated by a '/' character. So when
        // constructing a JSON Pointer one of whose components is a property name that
        // includes the '/' character, that character must be escaped with "~1". But now
        // the '~' character is also special, so it must be escaped with "~0".
        //
        // When escaping, the "~" replacement must come first. Otherwise, the string "/"
        // would translate to "~01" instead of the correct "~1". Similarly, when
        // unescaping, the "~1" replacement must come first.
        internal static string EscapeJsonPointer(this string propertyName)
        {
            return propertyName.Replace("~", "~0").Replace("/", "~1");
        }

        internal static string UnescapeJsonPointer(this string jPointer)
        {
            return jPointer.Replace("~1", "/").Replace("~0", "~");
        }
    }
}
