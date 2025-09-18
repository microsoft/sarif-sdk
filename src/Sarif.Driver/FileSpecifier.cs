// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class FileSpecifier : IArtifactProvider
    {
        public FileSpecifier(string specifier, bool recurse = false, IFileSystem fileSystem = null)
        {
            _specifier = specifier;
            _recurse = recurse;
            FileSystem = fileSystem ?? Microsoft.CodeAnalysis.Sarif.FileSystem.Instance;
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

        public IEnumerable<IEnumeratedArtifact> Artifacts
        {
            get
            {
                foreach (string file in Files)
                {
                    yield return new EnumeratedArtifact(FileSystem)
                    {
                        Uri = new Uri(file),
                    };
                }
            }
            set => throw new InvalidOperationException();
        }

        public ICollection<IEnumeratedArtifact> Skipped
        {
            get => Array.Empty<IEnumeratedArtifact>();
            set => throw new InvalidOperationException();
        }
        public IFileSystem FileSystem { get; set; }

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
                directory = $".{Path.DirectorySeparatorChar}";
            }

            AddFilesFromDirectory(directory, filter);
        }

        private void AddFilesFromDirectory(string dir, string filter)
        {
            if (FileSystem.DirectoryExists(dir))
            {
                foreach (string file in FileSystem.DirectoryGetFiles(dir, filter))
                {
                    AddFileToList(file);
                }

                if (_recurse)
                {
                    try
                    {
                        foreach (string subdir in FileSystem.DirectoryGetDirectories(dir))
                        {
                            // Skip subdirectories that are symbolic links to prevent infinite loops
                            // and avoid unintentionally following symbolic links during recursion
                            if (!FileSystem.IsSymbolicLink(subdir))
                            {
                                AddFilesFromDirectory(subdir, filter);
                            }
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
