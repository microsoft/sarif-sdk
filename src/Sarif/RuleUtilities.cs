// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class RuleUtilities
    {
        public static Result BuildResult(ResultLevel level, IAnalysisContext context, Region region, string ruleMessageId, params string[] arguments)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            ruleMessageId = NormalizeRuleMessageId(ruleMessageId, context.Rule.Id);

            Result result = new Result
            {
                RuleId = context.Rule.Id,
                RuleMessageId = ruleMessageId,

                Message = new Message
                {
                    Arguments = arguments
                },

                Level = level
            };

            string targetPath = context.TargetUri?.LocalPath;
            if (targetPath != null)
            {
                result.Locations = new List<Location> {
                    new Sarif.Location {
                        PhysicalLocation = new PhysicalLocation
                        {
                            FileLocation = new FileLocation
                            {
                                Uri = targetPath
                            },
                            Region = region
                        }
                    }
                };
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
            string ruleId,
            string prefix = null)
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

                string normalizedResourceName = NormalizeRuleMessageId(resourceName, ruleId, prefix);

                // We need to use the non-normalized key to retrieve the resource value
                dictionary[normalizedResourceName] = resourceValue;
            }

            return dictionary;
        }

        public static string NormalizeRuleMessageId(string ruleMessageId, string ruleId, string prefix = null)
        {
            if (ruleMessageId == null)
            {
                throw new ArgumentNullException(nameof(ruleMessageId));
            }

            if (!string.IsNullOrEmpty(ruleId) && ruleMessageId.StartsWith(ruleId + "_", StringComparison.Ordinal))
            {
                ruleMessageId = ruleMessageId.Substring(ruleId.Length + 1);
            }

            if (!string.IsNullOrEmpty(prefix) && ruleMessageId.StartsWith(prefix + "_", StringComparison.Ordinal))
            {
                ruleMessageId = ruleMessageId.Substring(prefix.Length + 1);
            }

            return ruleMessageId;
        }
    }
}
