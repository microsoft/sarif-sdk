// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public abstract class CommandBase
    {
        public static T ReadSarifFile<T>(IFileSystem fileSystem, string filePath, IContractResolver contractResolver = null)
        {
            var serializer = new JsonSerializer() { ContractResolver = contractResolver };

            using (JsonTextReader reader = new JsonTextReader(new StreamReader(fileSystem.OpenRead(filePath))))
            {
                return serializer.Deserialize<T>(reader);
            }
        }

        public static void WriteSarifFile<T>(IFileSystem fileSystem, T sarifFile, string outputName, Formatting formatting = Formatting.None, IContractResolver contractResolver = null)
        {
            var serializer = new JsonSerializer()
            {
                ContractResolver = contractResolver,
                Formatting = formatting
            };

            using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(fileSystem.Create(outputName))))
            {
                serializer.Serialize(writer, sarifFile);
            }
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
