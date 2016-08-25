// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Cli
{
    internal static class JPointerStringExtensions
    {
        internal static string AtProperty(this string jPointerValue, string propertyName)
        {
            return $"{jPointerValue}/{propertyName}";
        }

        internal static string AtIndex(this string jPointerValue, int index)
        {
            return $"{jPointerValue}/{index}";
        }
    }
}
