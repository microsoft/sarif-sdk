// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class UriUtilities
    {
        public const string FileScheme = "file";

        public static string WithColon(this string scheme)
        {
            scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            return $"{scheme}:";
        }
    }
}
