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
                string resourceValue = resourceManager.GetString(resourceName);

                string normalizedResourceName = NormalizeFormatSpecifierId(ruleId, resourceName);

                // We need to use the non-normalized key to retrieve the resource value
                dictionary[normalizedResourceName] = resourceValue;
            }

            return dictionary;
        }

        public static string NormalizeFormatSpecifierId(string ruleId, string formatSpecifierId)
        {
            if (!string.IsNullOrEmpty(ruleId) && formatSpecifierId.StartsWith(ruleId + "_"))
            {
                formatSpecifierId = formatSpecifierId.Substring(ruleId.Length + 1);
            }
            return formatSpecifierId;
        }
    }
}
