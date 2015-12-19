// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class NoteDescriptors
    {
        public static IRuleDescriptor AnalyzingTarget = new RuleDescriptor()
        {
            // A file is being analyzed.
            Id = "MSG1001",
            Name = nameof(AnalyzingTarget),
            FullDescription = SdkResources.InvalidTarget_Description,
            FormatSpecifiers = BuildDictionary(new string[] {
                    nameof(SdkResources.Analyzing),
                })
        };

        public static IRuleDescriptor InvalidTarget = new RuleDescriptor()
        {
            // A file was skipped as it does not appear to be a valid target for analysis.
            Id = "MSG1002",
            Name = nameof(InvalidTarget),
            FullDescription = SdkResources.InvalidTarget_Description,
            FormatSpecifiers = BuildDictionary(new string[] {
                    nameof(SdkResources.TargetNotAnalyzed_InvalidTarget),
                })
        };

        private static Dictionary<string, string> BuildDictionary(IEnumerable<string> resourceNames)
        {
            // Note this dictionary provides for case-insensitive keys
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string resourceName in resourceNames)
            {
                string resourceValue = SdkResources.ResourceManager.GetString(resourceName);
                dictionary[resourceName] = resourceValue;
            }

            return dictionary;
        }
    }
}