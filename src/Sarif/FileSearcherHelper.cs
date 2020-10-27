// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    internal static class FileSearcherHelper
    {
        /// <summary>
        /// This method will search in the environment variable for a specific file name.
        /// It will return the first file found.
        /// </summary>
        /// <param name="environmentVariable">Environment variable that we will look for</param>
        /// <param name="fileName">Name of the file that we will look for in the environment variable</param>
        /// <param name="fileSystem">An object that provides access to the file system.</param>
        /// <returns>Path to the file name or empty string.</returns>
        public static string SearchForFileInEnvironmentVariable(string environmentVariable, string fileName, IFileSystem fileSystem = null)
        {
            fileSystem ??= FileSystem.Instance;

            string variable = Environment.GetEnvironmentVariable(environmentVariable);
            if (string.IsNullOrEmpty(variable))
            {
                return null;
            }

            string[] paths = variable.Split(';');
            foreach (string path in paths)
            {
                string returnedPath = SearchForFileNameInPath(path, fileName, fileSystem);
                if (!string.IsNullOrEmpty(returnedPath))
                {
                    return returnedPath;
                }
            }

            return null;
        }

        /// <summary>
        /// This method will search for a file name in a specific path.
        /// </summary>
        /// <param name="path">Path where it will search.</param>
        /// <param name="fileName">Name of the file that it will search</param>
        /// <param name="fileSystem">An object that provides access to the file system.</param>
        /// <returns>Path to the file name or empty string.</returns>
        public static string SearchForFileNameInPath(string path, string fileName, IFileSystem fileSystem = null)
        {
            fileSystem ??= FileSystem.Instance;
            string filePath = $@"{path}\{fileName}";
            return fileSystem.FileExists(filePath) ? filePath : null;
        }
    }
}
