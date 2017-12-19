// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Sarif.Viewer
{
    static class StringExtensions
    {
        public static KeyValuePair<string, string> KeyWithValue(this string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }
    }
}
