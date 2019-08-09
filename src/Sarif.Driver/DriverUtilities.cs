// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

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
    }
}
