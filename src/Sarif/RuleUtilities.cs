// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class RuleUtilities
    {
        public static Result BuildResult(ResultLevel level, IAnalysisContext context, Region region, string formatId, params string[] arguments)
        {
            string[] messageArguments = arguments;

            formatId = RuleUtilities.NormalizeFormatId(context.Rule.Id, formatId);

            string targetPath = context.TargetUri?.LocalPath;

            Result result = new Result();

            result.RuleId = context.Rule.Id;

            if (!string.IsNullOrEmpty(targetPath))
            {
                // In the event of an analysis target, we always provide
                // the local path to this item as the 0th argument
                messageArguments = new string[arguments.Length + 1];
                messageArguments[0] = Path.GetFileName(targetPath);
                arguments.CopyTo(messageArguments, 1);
            }

            result.FormattedRuleMessage = new FormattedRuleMessage()
            {
                FormatId = formatId,
                Arguments = messageArguments
            };

            result.Level = level;

            if (targetPath != null)
            {
                result.Locations = new List<Location> {
                    new Sarif.Location {
                        AnalysisTarget = new PhysicalLocation
                        {
                            Uri = new Uri(targetPath),
                            Region = region
                        }
               }};
            }
            return result;
        }

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

                string normalizedResourceName = NormalizeFormatId(ruleId, resourceName);

                // We need to use the non-normalized key to retrieve the resource value
                dictionary[normalizedResourceName] = resourceValue;
            }

            return dictionary;
        }

        public static string NormalizeFormatId(string ruleId, string formatId)
        {
            if (!string.IsNullOrEmpty(ruleId) && formatId.StartsWith(ruleId + "_"))
            {
                formatId = formatId.Substring(ruleId.Length + 1);
            }
            return formatId;
        }
    }
}
