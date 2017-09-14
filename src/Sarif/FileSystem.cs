﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A wrapper class for accessing the file system.
    /// </summary>
    /// <remarks>
    /// Clients should use this class rather directly using the .NET file system classes, so they
    /// can mock the IFileSystem interface in unit tests.
    /// </remarks>
    public class FileSystem : IFileSystem
    {
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">
        /// The file to check.
        /// </param>
        /// <returns>
        /// true if the caller has the required permissions and path contains the name of an
        /// existing file; otherwise, false.
        /// </returns>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="path">
        /// The file or directory for which to obtain absolute path information.
        /// </param>
        /// <returns>
        /// The fully qualified location of <paramref name="path"/>, such as "C:\MyFile.txt".
        /// </returns>
        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Opens a binary file, reads all contents into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading.
        /// </param>
        /// <returns>
        /// A byte array containing the contents of the file
        /// </returns>
        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading. 
        /// </param>
        /// <returns>
        /// A string array containing all lines of the file.
        /// </returns>
        public string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }

        /// <summary>
        /// Opens a text file, reads all text in the file as a single string, and then closes
        /// the file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading. 
        /// </param>
        /// <returns>
        /// A string containing all text in the file.
        /// </returns>
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Opens a text file, reads all text in the file as a single string using the specified
        /// encoding, and then closes the file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading.
        /// </param>
        /// <param name="encoding">
        /// The encoding applied to the contents of the file.
        /// </param>
        /// <returns>
        /// A string containing all text in the file.
        /// </returns>
        public string ReadAllText(string path, Encoding encoding)
        {
            return File.ReadAllText(path, encoding);
        }
    }
}
