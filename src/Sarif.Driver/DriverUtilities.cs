// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public static class DriverUtilities
    {
        /// <summary>
        /// Returns a value indicating whether a set of output files can be created, and writes messages
        /// to the error stream if any of them cannot.
        /// </summary>
        /// <param name="outputFilePaths">
        /// A list of the paths to the output files.
        /// </param>
        /// <param name="force">
        /// true if the --force option was specified.
        /// </param>
        /// <param name="fileSystem">
        /// An object that provides access to the file system.
        /// </param>
        /// <returns>
        /// true if all the output files can be created; otherwise false.
        /// </returns>
        public static bool ReportWhetherOutputFilesCanBeCreated(IEnumerable<string> outputFilePaths, bool force, IFileSystem fileSystem)
        {
            bool canAllBeCreated = true;

            foreach (string outputFilePath in outputFilePaths)
            {
                canAllBeCreated &= ReportWhetherOutputFileCanBeCreated(outputFilePath, force, fileSystem);
            }

            return canAllBeCreated;
        }

        /// <summary>
        /// Returns a value indicating whether the output file can be created, and writes a message
        /// to the error stream if it cannot.
        /// </summary>
        /// <param name="outputFilePath">
        /// The path to the output file.
        /// </param>
        /// <param name="force">
        /// true if the --force option was specified.
        /// </param>
        /// <param name="fileSystem">
        /// An object that provides access to the file system.
        /// </param>
        /// <returns>
        /// true if the output file can be created; otherwise false.
        /// </returns>
        public static bool ReportWhetherOutputFileCanBeCreated(string outputFilePath, bool force, IFileSystem fileSystem)
        {
            bool canBeCreated = CanCreateOutputFile(outputFilePath, force, fileSystem);
            if (!canBeCreated)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.ERR997_OutputFileAlreadyExists,
                        outputFilePath));
            }

            return canBeCreated;
        }

        /// <summary>
        /// Returns a value indicating whether the output file can be created.
        /// </summary>
        /// <param name="outputFilePath">
        /// The path to the output file.
        /// </param>
        /// <param name="force">
        /// true if the --force option was specified.
        /// </param>
        /// <param name="fileSystem">
        /// An object that provides access to the file system.
        /// </param>
        /// <returns>
        /// true if the output file can be created; otherwise false.
        /// </returns>
        public static bool CanCreateOutputFile(string outputFilePath, bool force, IFileSystem fileSystem)
            => !fileSystem.FileExists(outputFilePath) || force;

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
        public static string GetOptionDescription<T>(string optionPropertyName)
            => GetOptionDescription(typeof(T), optionPropertyName);

        public static string GetOptionDescription(Type optionsType, string optionPropertyName)
        {
            PropertyInfo propertyInfo =
                optionsType.GetProperty(optionPropertyName, BindingFlags.Public | BindingFlags.Instance) ??
                throw new ArgumentException(
                    $"The type {optionsType.FullName} does not contain a public instance property named {optionPropertyName}.",
                    nameof(optionPropertyName));

            OptionAttribute optionAttribute =
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