// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>
        /// true if path refers to an existing directory; false if the directory does not exist
        /// or an error occurs when trying to determine if the specified directory exists.
        /// </returns>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

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
        /// Returns the names of subdirectories (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search. This string is not case-sensitive.
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) of subdirectories in the specified path,
        /// or an empty array if no directories are found.
        /// </returns>
        public IEnumerable<string> GetDirectoriesInDirectory(string path)
        {
            return Directory.GetDirectories(path);
        }

        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search pattern
        /// in the specified directory..
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search. This string is not case-sensitive.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter can contain
        /// a combination of valid literal path and wildcard (* and ?) characters, but it doesn't
        /// support regular expressions.
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the specified directory
        /// that match the specified search pattern, or an empty array if no files are found.
        /// </returns>
        public IEnumerable<string> GetFilesInDirectory(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
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
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>A DateTime structure set to the date and time that the specified file or directory was last written to.</returns>
        public DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
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

        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">The file for which to set the date and time information.</param>
        /// <param name="lastWriteTime">A DateTime containing the value to set for the last write date and time of path. This value is expressed in local time.</param>
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            File.SetLastWriteTime(path, lastWriteTime);
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">
        /// The file to write to.
        /// </param>
        /// <param name="bytes">
        /// The bytes to write to the file.
        /// </param>
        public void WriteAllBytes(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">
        /// The file to write to.
        /// </param>
        /// <param name="contents">
        /// The string to write to the file.
        /// </param>
        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        /// <summary>
        ///  Open an existing file for reading.
        /// </summary>
        /// <param name="path">File System path of file to open</param>
        /// <returns>Stream to read file</returns>
        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        /// <summary>
        ///  Create (or overwrite) a new file for writing.
        /// </summary>
        /// <param name="path">File System path of file to open</param>
        /// <returns>Stream to write file</returns>
        public Stream Create(string path)
        {
            return File.Create(path);
        }

        /// <summary>
        /// Sets the specified <see cref="FileAttributes"/> of the file on the specified path.
        /// </summary>
        /// <param name="path">
        /// The path to the file.
        /// </param>
        /// <param name="fileAttributes">
        /// A bitwise combination of the enumeration values.
        /// </param>
        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            File.SetAttributes(path, fileAttributes);
        }

        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they
        /// already exist.
        /// </summary>
        /// <param name="path">
        /// The directory to create.
        /// </param>
        /// <returns>
        /// An object that represents the directory at the specified path. This object is
        /// returned regardless of whether a directory at the specified path already exists.
        /// </returns>  
        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Deletes an empty directory from a specified path.
        /// </summary>
        /// <param name="path">
        /// The name of the empty directory to remove. This directory must be writable and empty.
        /// </param>
        public void DeleteDirectory(string path, bool recursive = false)
        {
            Directory.Delete(path, recursive);
        }
    }
}
