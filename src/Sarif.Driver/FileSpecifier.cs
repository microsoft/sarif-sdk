// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class FileSpecifier
    {
        public FileSpecifier(string specifier, bool recurse = false, string filter = "")
        {
            _recurse = recurse;

            if (filter != "" && specifier.EndsWith(filter, StringComparison.OrdinalIgnoreCase))
                filter = "";

            _specifier = Path.Combine(specifier, filter);
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

            string computedFilter = Path.GetFileName(expandedSpecifier);

            if (-1 == computedFilter.IndexOf("*"))
            {
                computedFilter = null;
            }
            else
            {
                expandedSpecifier = Path.GetDirectoryName(expandedSpecifier.Substring(0, expandedSpecifier.Length - computedFilter.Length));
            }

            if (File.Exists(expandedSpecifier))
            {
                AddFileToList(expandedSpecifier);
            }
            else
            {
                string dir;
                dir = expandedSpecifier;

                if (!Directory.Exists(expandedSpecifier))
                {
                    dir = Path.GetDirectoryName(expandedSpecifier);
                    computedFilter = Path.GetFileName(expandedSpecifier);
                }
                else
                {
                    _directories.Add(expandedSpecifier);
                }
                AddFilesFromDirectory(dir, computedFilter);
            }
        }

        private void AddFilesFromDirectory(string dir, string filter)
        {
            if (filter == null)
                return;

            if (Directory.Exists(dir))
            {
                foreach (string file in Directory.GetFiles(dir, filter))
                {
                    AddFileToList(file);
                }

                if (_recurse)
                {
                    try
                    {
                        foreach (string subdir in Directory.GetDirectories(dir))
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
