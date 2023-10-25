// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class OrderedFileSpecifier : IArtifactProvider
    {
        public OrderedFileSpecifier(string specifier,
                                    bool recurse = false,
                                    long maxFileSizeInKilobytes = long.MaxValue,
                                    CancellationToken cancellationToken = default,
                                    IFileSystem fileSystem = null)
        {
            this.specifier = specifier;
            this.recurse = recurse;
            this.maxFileSizeInKilobytes = maxFileSizeInKilobytes;
            this.cancellationToken = cancellationToken;
            FileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        private readonly bool recurse;
        private readonly string specifier;
        private readonly long maxFileSizeInKilobytes;
        private CancellationToken cancellationToken;

        public IEnumerable<IEnumeratedArtifact> Artifacts
        {
            get => EnumeratedArtifacts();
            set => throw new InvalidOperationException();
        }

        public ICollection<IEnumeratedArtifact> Skipped { get; set; }

        public IFileSystem FileSystem { get; set; }

        private IEnumerable<IEnumeratedArtifact> EnumeratedArtifacts()
        {
            string normalizedSpecifier = Environment.ExpandEnvironmentVariables(this.specifier);

            if (Uri.TryCreate(this.specifier, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                normalizedSpecifier = uri.GetFilePath();
            }

            string filter = Path.GetFileName(normalizedSpecifier);
            string directory = Path.GetDirectoryName(normalizedSpecifier);

            if (directory.Length == 0)
            {
                directory = $".{Path.DirectorySeparatorChar}";
            }

            directory = Path.GetFullPath(directory);

            if (!FileSystem.DirectoryExists(directory))
            {
                yield break;
            }

#if NETFRAMEWORK
            // .NET Framework: Directory.Enumerate with empty filter returns NO files.
            // .NET Core: Directory.Enumerate with empty filter returns all files in directory.
            // We will standardize on the .NET Core behavior.
            if (string.IsNullOrEmpty(filter))
            {
                filter = "*";
            }
#endif

            // allocating this here is a minor memory optimization.
            var sortedFilesBuffer = new SortedSet<string>();

            if (this.recurse)
            {
                foreach (IEnumeratedArtifact result in VisitAllSubdirectories(directory, filter, sortedFilesBuffer))
                {
                    yield return result;
                }
            }
            else
            {
                foreach (IEnumeratedArtifact result in VisitDirectory(directory, filter, sortedFilesBuffer))
                {
                    yield return result;
                }
            }
        }

        private IEnumerable<IEnumeratedArtifact> VisitAllSubdirectories(string directory, string filter, SortedSet<string> sortedFilesBuffer)
        {
            this.cancellationToken.ThrowIfCancellationRequested();

            var sortedDiskItems = new SortedSet<string>();

            foreach (IEnumeratedArtifact result in VisitDirectory(directory, filter, sortedFilesBuffer))
            {
                yield return result;
            }

            foreach (string childDirectory in FileSystem.DirectoryEnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                sortedDiskItems.Add(childDirectory);
            }

            foreach (string childDirectory in sortedDiskItems)
            {
                VisitAllSubdirectories(childDirectory, filter, sortedFilesBuffer);
            }
        }

        private IEnumerable<IEnumeratedArtifact> VisitDirectory(string targetDirectory, string filter, SortedSet<string> sortedFilesBuffer)
        {
            this.cancellationToken.ThrowIfCancellationRequested();
            sortedFilesBuffer.Clear();
#if NETFRAMEWORK
// .NET Framework: Directory.Enumerate with empty filter returns NO files.
            // .NET Core: Directory.Enumerate with empty filter returns all files in directory.
            // We will standardize on the .NET Core behavior.
            if (string.IsNullOrEmpty(filter))
            {
                filter = "*";
            }
#endif
            foreach (string file in FileSystem.DirectoryEnumerateFiles(targetDirectory, filter, SearchOption.TopDirectoryOnly))
            {
                this.cancellationToken.ThrowIfCancellationRequested();

                string fullFilePath = Path.Combine(targetDirectory, file);
                sortedFilesBuffer.Add(fullFilePath);
            }

            foreach (string file in sortedFilesBuffer)
            {
                yield return new EnumeratedArtifact(FileSystem)
                {
                    Uri = new Uri(file, UriKind.Absolute),
                };
            }
        }

        internal static IArtifactProvider Create(IEnumerable<string> specifiers, bool recurse, long maxFileSizeInKilobytes, IFileSystem fileSystem)
        {
            var orderedFileSpecifiers = new List<OrderedFileSpecifier>();

            foreach (string specifier in specifiers)
            {
                orderedFileSpecifiers.Add(new OrderedFileSpecifier(specifier, recurse, maxFileSizeInKilobytes, fileSystem: fileSystem));
            }

            return new AggregatingArtifactsProvider(fileSystem)
            {
                Providers = orderedFileSpecifiers
            };
        }
    }
}
