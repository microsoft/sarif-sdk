// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class CommandBase
    {
        public const int SUCCESS = 0;
        public const int FAILURE = 1;

        // TBD delete this!
        protected virtual IFileSystem FileSystem { get; set; }

        // TODO:  Removing this constructor broke the one of AbsoluteUriCommand entirely but all unit tests were passing
        // Add unit tests that will break when this constructor is deleted or malfunctioning.
        // #2268 https://github.com/microsoft/sarif-sdk/issues/2268
        public CommandBase(IFileSystem fileSystem = null)
        {
            this.FileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        //  TODO:  What's the point of having a bunch of static methods in an abstract class?
        //  We even have a static class, "CommandUtilities" which seems like the more appropriate
        //  place for these to go.
        //  #2269 https://github.com/microsoft/sarif-sdk/issues/2269
        protected static bool ValidateNonNegativeCommandLineOption<T>(long optionValue, string optionName)
        {
            bool valid = true;

            if (optionValue < 0)
            {
                string optionDescription = DriverUtilities.GetOptionDescription<T>(optionName);
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        DriverResources.OptionValueMustBeNonNegative,
                        optionDescription));
                valid = false;
            }

            return valid;
        }

        public static T ReadSarifFile<T>(IFileSystem fileSystem, string filePath, IContractResolver contractResolver = null)
        {
            var serializer = new JsonSerializer() { ContractResolver = contractResolver };

            using (JsonTextReader reader = new JsonTextReader(new StreamReader(fileSystem.FileOpenRead(filePath))))
            {
                return serializer.Deserialize<T>(reader);
            }
        }

        public static void WriteSarifFile<T>(IFileSystem fileSystem,
                                             T sarifFile,
                                             string outputName,
                                             bool minify = false,
                                             IContractResolver contractResolver = null)
        {
            WriteSarifFile(fileSystem, sarifFile, outputName, minify ? 0 : Formatting.Indented, contractResolver);
        }

        public static void WriteSarifFile<T>(IFileSystem fileSystem,
                                             T sarifFile,
                                             string outputName,
                                             Formatting formatting = Formatting.None,
                                             IContractResolver contractResolver = null)
        {
            var serializer = new JsonSerializer()
            {
                ContractResolver = contractResolver,
                Formatting = formatting
            };

            using (var writer = new JsonTextWriter(new StreamWriter(fileSystem.FileCreate(outputName))))
            {
                serializer.Serialize(writer, sarifFile);
            }
        }

        public static HashSet<string> CreateTargetsSet(IEnumerable<string> targetSpecifiers, bool recurse, IFileSystem fileSystem)
        {
            HashSet<string> targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string specifier in targetSpecifiers)
            {
                string normalizedSpecifier = specifier;

                if (Uri.TryCreate(specifier, UriKind.RelativeOrAbsolute, out Uri uri))
                {
                    normalizedSpecifier = uri.GetFilePath();
                }

                // Currently, we do not filter on any extensions.
                var fileSpecifier = new FileSpecifier(normalizedSpecifier, recurse, fileSystem);
                foreach (string file in fileSpecifier.Files) { targets.Add(file); }
            }

            return targets;
        }
    }
}
