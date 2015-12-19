// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class RuleUtilities
    {
        public static Dictionary<string, string> BuildDictionary(
            ResourceManager resourceManager, 
            IEnumerable<string> resourceNames, 
            string ruleId = null)
        {
            // Note this dictionary provides for case-insensitive keys
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string resourceName in resourceNames)
            {
                string normalizedResourceName = resourceName;
                if (!string.IsNullOrEmpty(ruleId) && normalizedResourceName.StartsWith(ruleId + "_"))
                {
                    normalizedResourceName = resourceName.Substring(ruleId.Length + 1);
                }
                // We need to use the non-normalized key to retrieve the resource value
                string resourceValue = resourceManager.GetString(resourceName);
                dictionary[normalizedResourceName] = resourceValue;
            }

            return dictionary;
        }
    }
}
