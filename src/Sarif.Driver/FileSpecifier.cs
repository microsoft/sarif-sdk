// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class FileSpecifier
    {
        private readonly IFileSystem _fileSystem;

        public FileSpecifier(string specifier, bool recurse = false, IFileSystem fileSystem = null)
        {
            _specifier = specifier;
            _recurse = recurse;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        private readonly bool _recurse;
        private readonly string _specifier;
        private List<string> _files;
        private List<string> _directories;

        public IList<string> Files
        {
            get { return _files ?? BuildFiles(); }
        }

        public IList<string> Directories
        {
            get { return _directories ?? BuildDirectories(); }
        }

        private List<string> BuildDirectories()
        {
            BuildFilesAndDirectoriesList();
            return _directories;
        }

        private List<string> BuildFiles()
        {
            BuildFilesAndDirectoriesList();
            return _files;
        }

        private void BuildFilesAndDirectoriesList()
        {
            string expandedSpecifier;

            _files = new List<string>();
            _directories = new List<string>();

            expandedSpecifier = Environment.ExpandEnvironmentVariables(_specifier);

            string filter = Path.GetFileName(expandedSpecifier);
            string directory = Path.GetDirectoryName(expandedSpecifier);

            if (directory.Length == 0)
            {
                directory = @".\";
            }

            AddFilesFromDirectory(directory, filter);
        }

        private void AddFilesFromDirectory(string dir, string filter)
        {
            if (_fileSystem.DirectoryExists(dir))
            {
                foreach (string file in _fileSystem.GetFilesInDirectory(dir, filter))
                {
                    AddFileToList(file);
                }

                if (_recurse)
                {
                    try
                    {
                        foreach (string subdir in _fileSystem.GetDirectoriesInDirectory(dir))
                        {
                            AddFilesFromDirectory(subdir, filter);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.Error.WriteLineAsync("Unauthorized access exception for directory: " + dir);
                    }
                }
            }
        }

        private void AddFileToList(string expandedSpecifier)
        {
            _files.Add(Path.GetFullPath(expandedSpecifier));
        }
    }
}
