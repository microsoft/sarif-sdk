// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class CommandUtilities
    {
        internal static string GetTransformedOutputFileName(SingleFileOptionsBase options)
        {
            string filePath = Path.GetFullPath(options.InputFilePath);

            if (options.Inline)
            {
                return filePath;
            }

            if (!String.IsNullOrEmpty(options.OutputFilePath))
            {
                return options.OutputFilePath;
            }

            const string TransformedExtension = ".transformed.sarif";
            string extension = Path.GetExtension(filePath);

            // For an input file named MyFile.sarif, returns MyFile.transformed.sarif.
            if (extension.Equals(".sarif", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + TransformedExtension);
            }

            // For an input file named MyFile.json, return MyFile.json.transformed.sarif.
            return Path.GetFullPath(options.InputFilePath + TransformedExtension);
        }

        /// <summary>
        /// Constructs a description of a command line option.
        /// </summary>
        /// <typeparam name="T">
        /// The type that defines the property corresponding to the command line option.
        /// </typeparam>
        /// <param name="optionPropertyName">
        /// The name of the property corresponding to the command line option.
        /// </param>
        /// <returns>
        /// A description of the specified command line option in the format "shortName", "longName",
        /// or "shortName, longName", depending on which names are available.
        /// </returns>
        /// <remarks>
        /// The CommandLine package defines <see cref="CommandLine.OptionAttribute"/> to mark
        /// properties that correspond to command line options. CommandLine performs some validation,
        /// but sometimes it is necessary to perform additional validation. In that case, it is
        /// desirable for the validation message to refer to the invalid parameter in the same
        /// format that CommandLine itself does.
        /// </remarks>
        internal static string GetOptionDescription<T>(string optionPropertyName)
            => GetOptionDescription(typeof(T), optionPropertyName);

        internal static string GetOptionDescription(Type optionsType, string optionPropertyName)
        {
            PropertyInfo propertyInfo =
                optionsType.GetProperty(optionPropertyName, BindingFlags.Public | BindingFlags.Instance) ??
                throw new ArgumentException(
                    $"The type {optionsType.FullName} does not contain a public instance property named {optionPropertyName}.",
                    nameof(optionPropertyName));

            var optionAttribute =
                propertyInfo.GetCustomAttribute<OptionAttribute>() ??
                throw new ArgumentException(
                    $"The {optionPropertyName} property of the type {optionsType.FullName} does not define a command line option.",
                    nameof(optionPropertyName));

            var builder = new StringBuilder(optionAttribute.ShortName);
            if (builder.Length > 0 && optionAttribute.LongName != string.Empty) { builder.Append(", "); }
            builder.Append(optionAttribute.LongName);

            return builder.ToString();
        }
    }
}
