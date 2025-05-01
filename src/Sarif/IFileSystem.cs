// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// An interface for accessing the file system.
    /// </summary>
    /// <remarks>
    /// Clients wishing to access the file system should instantiate a FileSystem
    /// object rather than directly using the .NET file system and other classes,
    ///  so they can mock the IFileSystem interface in unit tests.
    /// </remarks>
    public interface IFileSystem
    {
        /// <summary>
        /// Loads an assembly given its file name or path.
        /// </summary>
        /// <param name="assemblyFile">
        /// The name or path of the file that contains the manifest of the assembly.
        /// </param>
        /// <returns>
        /// The loaded assembly.
        /// </returns>
        Assembly AssemblyLoadFrom(string assemblyFile);

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
        DirectoryInfo DirectoryCreateDirectory(string path);

        /// <summary>
        /// Deletes an empty directory from a specified path.
        /// </summary>
        /// <param name="path">
        /// The name of the empty directory to remove. This directory must be writable and empty.
        /// </param>
        void DirectoryDelete(string path, bool recursive = false);

        /// <summary>
        /// Returns an enumerable collection of directory full names in a specified path.
        /// </summary>
        /// <param name="path">
        /// Thee relative or absolute path to the directory to search. This string is not case-sensitive.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of directories in path. This parameter can contain
        /// a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.
        /// </param>
        /// <param name="searchOption">
        /// One of the enumeration values that specifies whether the search operation should include only the current
        /// directory or should include all subdirectories. The default value is TopDirectoryOnly.
        /// </param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the directories in the directory specified
        /// by path and that match the specified search pattern and search option.
        /// </returns>
        IEnumerable<string> DirectoryEnumerateDirectories(string path, string searchPattern, SearchOption searchOption);

        /// <summary>
        /// Returns an enumerable collection of file names in a specified path.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search. This string is not case-sensitive.
        /// </param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the files in the directory
        /// the directory specified by path.
        /// </returns>
        IEnumerable<string> DirectoryEnumerateFiles(string path);

        /// <summary>
        /// Returns an enumerable collection of full file names that match a search pattern in a
        /// specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search. This string is not case-sensitive.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter can contain a
        /// combination of valid literal path and wildcard (* and ?) characters, but it doesn't support
        /// regular expressions.
        /// </param>
        /// <param name="searchOption">
        /// One of the enumeration values that specifies whether the search operation should include only
        /// the current directory or should include all subdirectories. The default value is TopDirectoryOnly.
        /// </param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the files in the directory
        /// specified by path and that match the specified search pattern and search option.
        /// </returns>
        IEnumerable<string> DirectoryEnumerateFiles(string path, string searchPattern, SearchOption searchOption);

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>
        /// true if path refers to an existing directory; false if the directory does not exist
        /// or an error occurs when trying to determine if the specified directory exists.
        /// </returns>
        bool DirectoryExists(string path);

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
        IEnumerable<string> DirectoryGetDirectories(string path);

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
        IEnumerable<string> DirectoryGetFiles(string path, string searchPattern);

        /// <summary>
        /// Gets or sets the fully qualified path of the current working directory.
        /// </summary>
        /// <returns>
        /// A string containing a directory path.
        /// </returns>
        string EnvironmentCurrentDirectory { get; set; }

        /// <summary>
        ///  Create (or overwrite) a new file for writing.
        /// </summary>
        /// <param name="path">File System path of file to open</param>
        /// <returns>Stream to write file</returns>
        Stream FileCreate(string path);

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">
        /// The name of the file to be deleted. Wildcard characters are not supported.
        /// </param>
        void FileDelete(string path);

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
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">
        /// The file or directory for which to obtain write date and time information.
        /// </param>
        /// <returns>
        /// A DateTime structure set to the date and time that the specified file or directory was last written to.
        /// </returns>
        DateTime FileGetLastWriteTime(string path);

        /// <summary>
        /// Opens a binary file, reads all contents into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading.
        /// </param>
        /// <returns>
        /// A byte array containing the contents of the file
        /// </returns>
        byte[] FileReadAllBytes(string path);

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading.
        /// </param>
        /// <returns>
        /// A string array containing all lines of the file.
        /// </returns>
        string[] FileReadAllLines(string path);

        /// <summary>
        /// Reads the lines of a file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading.
        /// </param>
        /// <returns>
        /// All the lines of the file, or the lines that are the result of a query.
        /// </returns>
        IEnumerable<string> FileReadLines(string path);

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
        string FileReadAllText(string path);

        /// <summary>
        /// Uses <see cref="FileStream"/> to get the size of a file in bytes.
        /// </summary>
        /// <param name="path">
        /// The fully qualified name or relative name of the file.
        /// </param>
        /// <returns>
        /// A long representing the size of the file in bytes.
        /// </returns>
        long FileStreamLength(string path);

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
        string FileReadAllText(string path, Encoding encoding);

        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">The file for which to set the date and time information.</param>
        /// <param name="lastWriteTime">A DateTime containing the value to set for the last write date and time of path. This value is expressed in local time.</param>
        void FileSetLastWriteTime(string path, DateTime lastWriteTime);

        /// <summary>
        /// Creates a new file, writes the specified bytes to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">
        /// The file to write to.
        /// </param>
        /// <param name="bytes">
        /// The bytes to write to the file.
        /// </param>
        void FileWriteAllBytes(string path, byte[] bytes);

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
        void FileWriteAllText(string path, string contents);

        /// <summary>
        ///  Open an existing file for reading.
        /// </summary>
        /// <param name="path">File System path of file to open</param>
        /// <returns>Stream to read file</returns>
        Stream FileOpenRead(string path);

        /// <summary>
        /// Sets the specified <see cref="FileAttributes"/> of the file on the specified path.
        /// </summary>
        /// <param name="path">
        /// The path to the file.
        /// </param>
        /// <param name="fileAttributes">
        /// A bitwise combination of the enumeration values.
        /// </param>
        void FileSetAttributes(string path, FileAttributes fileAttributes);

        /// <summary>
        /// Uses <see cref="FileInfo"/> to calculate the size of a file in bytes.
        /// </summary>
        /// <param name="path">
        /// The fully qualified name or relative name of the file.
        /// </param>
        /// <returns>
        /// A long representing the size of the file in bytes.
        /// </returns>
        long FileInfoLength(string path);

        /// <summary>
        /// Uses <see cref="FileInfo"/> to determine whether a file is a symbolic link.
        /// </summary>
        /// <param name="path">
        /// The fully qualified name or relative path of the file.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the file is a symbolic link.
        /// </returns>
        bool IsSymbolicLink(string path);

        /// <summary>
        /// Returns a <see cref="FileVersionInfo"/> representing the version information associated with the specified file.
        /// </summary>
        /// <param name="path">The fully qualified path and name of the file to retrieve the version information for.</param>
        /// <returns>A <see cref="FileVersionInfo"/> containing information about the file. If the file did not
        /// contain version information, the FileVersionInfo contains only the name of the file requested.</returns>
        FileVersionInfo FileVersionInfoGetVersionInfo(string fileName);

        /// <summary>
        /// Combines an array of strings into a path.
        /// </summary>
        /// <param name="paths">
        /// An array of parts of the path.
        /// </param>
        /// <returns>
        /// The combined path.
        /// </returns>
        string PathCombine(params string[] paths);

        /// <summary>
        /// Returns the directory information for the specified path.
        /// </summary>
        /// <param name="path">
        /// The path of a file or directory.
        /// </param>
        /// <returns>
        /// Directory information for path, or null if path denotes a root directory or is null. Returns <see cref="string.Empty"/> if path does not contain directory information.
        /// </returns>
        string PathGetDirectoryName(string path);

        /// <summary>
        /// Returns the absolute path for the specified path.
        /// </summary>
        /// <param name="path">
        /// The path of a file or directory.
        /// </param>
        /// <returns>
        /// The fully qualified location of <paramref name="path"/>, such as "C:\MyFile.txt".
        /// </returns>
        string PathGetFullPath(string path);

        /// <summary>
        /// Returns the file name of the specified path string without the extension.
        /// </summary>
        /// <param name="path">
        /// The path of the file.
        /// </param>
        /// <returns>
        /// The string returned by <see cref = "Path.GetFileName(string)"/>, minus the last period (.) and all characters following it.
        /// </returns>
        string PathGetFileNameWithoutExtension(string path);

        /// <summary>
        /// Returns the extension of the given path. The returned value includes the period (".") character of the extension
        /// except when you have a terminal period when you get String.Empty, such as ".exe" or ".cpp".
        ///
        /// While the latest version of Path.GetExtension will not throw if the path contains any illegal characters, older
        /// versions of .NET, some of which are still in use, will.  This method acts like the newer version and will not throw.
        /// </summary>
        /// <param name="path">The path to extract the extension from</param>
        /// <returns>The file extension or null if the given path is null or if the given path does not include an extension.</returns>
        string PathGetExtension(string path);
    }
}
