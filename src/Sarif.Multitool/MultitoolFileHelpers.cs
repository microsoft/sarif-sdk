// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Helper functions for the multitool.
    /// </summary>
    static class MultitoolFileHelpers
    {
        public static T ReadSarifFile<T>(string filePath)
        {
            return ReadSarifFile<T>(filePath, SarifContractResolver.Instance);
        }

        public static T ReadSarifFile<T>(string filePath, IContractResolver contractResolver)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver
            };

            string logText = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<T>(logText, settings);
        }

        public static void WriteSarifFile<T>(T sarifFile, string outputName, Formatting formatting)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = formatting
            };

            File.WriteAllText(outputName, JsonConvert.SerializeObject(sarifFile, settings));
        }

        public static HashSet<string> CreateTargetsSet(IEnumerable<string> targetSpecifiers, bool recurse)
        {
            HashSet<string> targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string specifier in targetSpecifiers)
            {
                string normalizedSpecifier = specifier;

                Uri uri;
                if (Uri.TryCreate(specifier, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (uri.IsAbsoluteUri && (uri.IsFile || uri.IsUnc))
                    {
                        normalizedSpecifier = uri.LocalPath;
                    }
                }
                // Currently, we do not filter on any extensions.
                var fileSpecifier = new FileSpecifier(normalizedSpecifier, recurse);
                foreach (string file in fileSpecifier.Files) { targets.Add(file); }
            }

            return targets;
        }
    }
}
