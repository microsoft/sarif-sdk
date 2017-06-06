﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class RuleUtilities
    {
        public static Result BuildResult(ResultLevel level, IAnalysisContext context, Region region, string formatId, params string[] arguments)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            formatId = RuleUtilities.NormalizeFormatId(context.Rule.Id, formatId);

            Result result = new Result
            {
                RuleId = context.Rule.Id,

                FormattedRuleMessage = new FormattedRuleMessage()
                {
                    FormatId = formatId,
                    Arguments = arguments
                },

                Level = level
            };

            string targetPath = context.TargetUri?.LocalPath;
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

            if (level == ResultLevel.Warning)
            {
                context.RuntimeErrors |= RuntimeConditions.OneOrMoreWarningsFired;
            }

            if (level == ResultLevel.Error)
            {
                context.RuntimeErrors |= RuntimeConditions.OneOrMoreErrorsFired;
            }

            return result;
        }

        public static Dictionary<string, string> BuildDictionary(
            ResourceManager resourceManager, 
            IEnumerable<string> resourceNames, 
            string ruleId)
        {
            //validation
            if (resourceNames == null)
            {
                throw new ArgumentNullException(nameof(resourceNames));
            }

            if (resourceManager == null)
            {
                throw new ArgumentNullException(nameof(resourceManager));
            }

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
            if (formatId == null)
            {
                throw new ArgumentNullException(nameof(formatId));
            }

            if (!string.IsNullOrEmpty(ruleId) && formatId.StartsWith(ruleId + "_", StringComparison.Ordinal))
            {
                formatId = formatId.Substring(ruleId.Length + 1);
            }
            return formatId;
        }
    }
}
