// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class RuleUtilities
    {
        public static Result BuildResult(ResultKind messageKind, IAnalysisContext context, Region region, string formatSpecifierId, params string[] arguments)
        {
            string[] messageArguments = arguments;

            formatSpecifierId = RuleUtilities.NormalizeFormatSpecifierId(context.Rule.Id, formatSpecifierId);

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

            result.FormattedMessage = new FormattedMessage()
            {
                SpecifierId = formatSpecifierId,
                Arguments = messageArguments
            };

            result.Kind = messageKind;

            if (targetPath != null)
            {
                result.Locations = new[] {
                new Sarif.Location {
                    AnalysisTarget = new[]
                    {
                        new PhysicalLocationComponent
                        {
                            // Why? When NewtonSoft serializes this Uri, it will use the
                            // original string used to construct the Uri. For a file path, 
                            // this will be the local file path. We want to persist this 
                            // information using the file:// protocol rendering, however.
                            Uri = targetPath.CreateUriForJsonSerialization(),
                            MimeType = context.MimeType,
                            Region = region
                        },
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
