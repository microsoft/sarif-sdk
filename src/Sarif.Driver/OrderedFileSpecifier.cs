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
            this.recurse = recurse;
            this.specifier = specifier;
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
            var directories = new Queue<string>();

            if (!FileSystem.DirectoryExists(directory))
            {
                yield break;
            }

            if (this.recurse)
            {
                EnqueueAllDirectories(directories, directory);
            }
            else
            {
                directories.Enqueue(directory);
            }

            var sortedFiles = new SortedSet<string>();

            while (directories.Count > 0)
            {
                sortedFiles.Clear();
                this.cancellationToken.ThrowIfCancellationRequested();

                directory = Path.GetFullPath(directories.Dequeue());

#if NETFRAMEWORK
                // .NET Framework: Directory.Enumerate with empty filter returns NO files.
                // .NET Core: Directory.Enumerate with empty filter returns all files in directory.
                // We will standardize on the .NET Core behavior.
                if (string.IsNullOrEmpty(filter))
                {
                    filter = "*";
                }
#endif

                foreach (string file in FileSystem.DirectoryEnumerateFiles(directory, filter, SearchOption.TopDirectoryOnly))
                {
                    this.cancellationToken.ThrowIfCancellationRequested();

                    string fullFilePath = Path.Combine(directory, file);

                    if (!IsTargetWithinFileSizeLimit(file, this.maxFileSizeInKilobytes, FileSystem, out long fileSizeInKb))
                    {
                        Skipped ??= new List<IEnumeratedArtifact>();
                        Skipped.Add(new EnumeratedArtifact(FileSystem)
                        {
                            Uri = new Uri(fullFilePath, UriKind.Absolute),
                        });
                    }
                    else
                    {
                        sortedFiles.Add(fullFilePath);
                    }
                }

                foreach (string file in sortedFiles)
                {
                    yield return new EnumeratedArtifact(FileSystem)
                    {
                        Uri = new Uri(file, UriKind.Absolute),
                    };
                }
            }
        }

        internal static bool IsTargetWithinFileSizeLimit(string path, long maxFileSizeInKB, IFileSystem fileSystem, out long fileSizeInKb)
        {
            fileSizeInKb = 0;
            long size = fileSystem.FileInfoLength(path);
            if (size == 0) { return false; };

            size = Math.Min(long.MaxValue - 1023, size);
            fileSizeInKb = (size + 1023) / 1024;
            return fileSizeInKb <= maxFileSizeInKB;
        }

        private void EnqueueAllDirectories(Queue<string> queue, string directory)
        {
            this.cancellationToken.ThrowIfCancellationRequested();

            var sortedDiskItems = new SortedSet<string>();

            queue.Enqueue(directory);
            foreach (string childDirectory in FileSystem.DirectoryEnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                sortedDiskItems.Add(childDirectory);
            }

            foreach (string childDirectory in sortedDiskItems)
            {
                EnqueueAllDirectories(queue, childDirectory);
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
