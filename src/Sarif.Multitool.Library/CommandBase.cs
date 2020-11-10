﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public abstract class CommandBase
    {
        public const int SUCCESS = 0;
        public const int FAILURE = 1;

        protected static bool ValidateNonNegativeCommandLineOption<T>(long optionValue, string optionName)
        {
            bool valid = true;

            if (optionValue < 0)
            {
                string optionDescription = DriverUtilities.GetOptionDescription<T>(optionName);
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.OptionValueMustBeNonNegative,
                        optionDescription));
                valid = false;
            }

            return valid;
        }

        public static T ReadSarifFile<T>(IFileSystem fileSystem, string filePath, IContractResolver contractResolver = null)
        {
            if (typeof(T) == typeof(SarifLog))
            {
                return (T)(object)SarifLog.Load(fileSystem.OpenRead(filePath), filePath);
            }
            else
            {
                var serializer = new JsonSerializer()
                {
                    ContractResolver = contractResolver
                };

                using (JsonTextReader reader = new JsonTextReader(new StreamReader(fileSystem.OpenRead(filePath))))
                {
                    return serializer.Deserialize<T>(reader);
                }
            }
        }

        public static void WriteSarifFile<T>(IFileSystem fileSystem, T sarifFile, string outputName, Formatting formatting = Formatting.None, IContractResolver contractResolver = null)
        {
            if (typeof(T) == typeof(SarifLog))
            {
                ((SarifLog)(object)sarifFile).Save(fileSystem.FileCreate(outputName), ConvertFormat(formatting, outputName));
            }
            else
            {
                var serializer = new JsonSerializer()
                {
                    ContractResolver = contractResolver,
                    Formatting = formatting
                };

                using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(fileSystem.FileCreate(outputName))))
                {
                    serializer.Serialize(writer, sarifFile);
                }
            }
        }

        private static SarifFormat ConvertFormat(Formatting formatting, string outputName)
        {
            if (outputName.ToLowerInvariant().EndsWith(".bsoa"))
            {
                return SarifFormat.BSOA;
            }

            return (formatting == Formatting.Indented ? SarifFormat.IndentedJSON : SarifFormat.JSON);
        }

        public static HashSet<string> CreateTargetsSet(IEnumerable<string> targetSpecifiers, bool recurse, IFileSystem fileSystem)
        {
            HashSet<string> targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string specifier in targetSpecifiers)
            {
                string normalizedSpecifier = specifier;

                if (Uri.TryCreate(specifier, UriKind.RelativeOrAbsolute, out Uri uri))
                {
                    if (uri.IsAbsoluteUri && (uri.IsFile || uri.IsUnc))
                    {
                        normalizedSpecifier = uri.LocalPath;
                    }
                }
                // Currently, we do not filter on any extensions.
                var fileSpecifier = new FileSpecifier(normalizedSpecifier, recurse, fileSystem);
                foreach (string file in fileSpecifier.Files) { targets.Add(file); }
            }

            return targets;
        }
    }
}
