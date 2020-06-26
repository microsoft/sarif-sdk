// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class StringExtensions
    {
        public static JToken ToJToken(this string pointer, JToken parentToken)
        {
            var jsonPointer = new JsonPointer(pointer);
            JToken jToken = jsonPointer.Evaluate(parentToken);
            return jToken;
        }
    }
}
