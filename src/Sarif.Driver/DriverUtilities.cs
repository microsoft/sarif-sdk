// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public static class DriverUtilities
    {
        /// <summary>
        /// Verifies that the output file either does not exist or is allowed to be overrwritten.
        /// </summary>
        /// <param name="outputFilePath">
        /// The path to the output file.
        /// </param>
        /// <param name="force">
        /// true if the --force option was specified, allowing the output file to be overwritten;
        /// otherwise false.
        /// </param>
        /// <param name="fileSystem">
        /// An object that provides access to the file system.
        /// </param>
        public static void VerifyOutputFileCanBeCreated(string outputFilePath, bool force, IFileSystem fileSystem)
        {
            if (fileSystem.FileExists(outputFilePath) && !force)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        DriverResources.OutputFileAlreadyExists,
                        outputFilePath));
            }
        }
    }
}
