// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// An interface for accessing the file system.
    /// </summary>
    /// <remarks>
    /// Clients wishing to access the file system should instantiate a FileSystem object rather
    /// than directly using the .NET file system classes, so they can mock the IFileSystem
    /// interface in unit tests.
    /// </remarks>
    public interface IFileSystem
    {
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">
        /// The file to check.
        /// </param>
        /// <returns>
        /// true if the caller has the required permissions and <paramref name="path"/> contains
        /// the name of an existing file; otherwise, false.
        /// </returns>
        bool FileExists(string path);

        /// <summary>
        /// Returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="path">
        /// The file or directory for which to obtain absolute path information.
        /// </param>
        /// <returns>
        /// The fully qualified location of <paramref name="path"/>, such as "C:\MyFile.txt".
        /// </returns>
        string GetFullPath(string path);

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading. 
        /// </param>
        /// <returns>
        /// A string array containing all lines of the file.
        /// </returns>
        string[] ReadAllLines(string path);
    }
}
