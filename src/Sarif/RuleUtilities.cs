// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class RuleUtilities
    {
        public static Result BuildResult(ResultKind kind, IAnalysisContext context, Region region, string ruleMessageId, params string[] arguments)
        {
            // If kind indicates a failure, but we have no explicit failure
            // level, we'll fall back to the default of Warning
            FailureLevel level = (kind != ResultKind.Fail)
                ? FailureLevel.None
                : FailureLevel.Warning;

            return BuildResult(level, kind, context, region, ruleMessageId, arguments);
        }

        public static Result BuildResult(FailureLevel level, IAnalysisContext context, Region region, string ruleMessageId, params string[] arguments)
        {
            // If we have a failure level, the kind is Fail, otherwise None.
            // A message of kind == none and failure level of none is a trace
            // message, pure and simple.
            ResultKind kind = (level != FailureLevel.None)
                ? ResultKind.Fail 
                : ResultKind.None;

            return BuildResult(level, kind, context, region, ruleMessageId, arguments);
        }

        public static Result BuildResult(FailureLevel level, ResultKind kind, IAnalysisContext context, Region region, string ruleMessageId, params string[] arguments)
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

                Message = new Message
                {
                    MessageId = ruleMessageId,
                    Arguments = arguments
                },

                Level = level,
                Kind = kind
            };

            string targetPath = context.TargetUri?.LocalPath;
            if (targetPath != null)
            {
                result.Locations = new List<Location> {
                    new Sarif.Location {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(targetPath)
                            },
                            Region = region
                        }
                    }
                };
            }

            if (level == FailureLevel.Warning)
            {
                context.RuntimeErrors |= RuntimeConditions.OneOrMoreWarningsFired;
            }

            if (level == FailureLevel.Error)
            {
                context.RuntimeErrors |= RuntimeConditions.OneOrMoreErrorsFired;
            }

            return result;
        }

        public static Dictionary<string, MultiformatMessageString> BuildDictionary(
            ResourceManager resourceManager,
            IEnumerable<string> resourceNames,
            string ruleId)
        {
            if (resourceNames == null)
            {
                throw new ArgumentNullException(nameof(resourceNames));
            }

            if (resourceManager == null)
            {
                throw new ArgumentNullException(nameof(resourceManager));
            }

            // Note this dictionary provides for case-insensitive keys
            var dictionary = new Dictionary<string, MultiformatMessageString>(StringComparer.OrdinalIgnoreCase);

            foreach (string resourceName in resourceNames)
            {
                string resourceValue = resourceManager.GetString(resourceName);

                string normalizedResourceName = NormalizeRuleMessageId(resourceName, ruleId);

                // We need to use the non-normalized key to retrieve the resource value
                dictionary[normalizedResourceName] = new MultiformatMessageString
                {
                    Text = resourceValue
                };
            }

            // We need to return null here, otherwise this empty dictionary will serialize to SARIF logs unnecessarily
            return dictionary.Count > 0 ? dictionary : null;
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