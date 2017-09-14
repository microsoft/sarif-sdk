// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public static class JTokenExtensions
    {
        public static bool HasProperty(this JToken token, string propertyName)
        {
            return token.Children<JProperty>()
                .Any(jp => jp.Name.Equals(propertyName, StringComparison.Ordinal));
        }
    }
}