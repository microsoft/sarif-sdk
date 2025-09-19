// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

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

        internal const int ChannelCapacity = 10 * 1024; // max ~2.5 MB memory given 256 char max path length.
        private readonly bool recurse;
        private readonly string specifier;
        private readonly long maxFileSizeInKilobytes;
        private CancellationToken cancellationToken;
        private volatile bool isEnumerationEnded = false;

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

            if (!filter.Contains("*") && filter.Length != 0 && !FileSystem.FileExists(normalizedSpecifier))
            {
                directory = $"{normalizedSpecifier}{Path.DirectorySeparatorChar}";
                filter = "*";
            }

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

            var filesToProcessChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(ChannelCapacity)
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait
            });

            Task directoryEnumerationTask;

            directoryEnumerationTask = Task.Run(() =>
            {
                try
                {
                    if (this.recurse)
                    {
                        EnqueueAllFilesUnderDirectory(directory, filesToProcessChannel.Writer, filter, new SortedSet<string>(StringComparer.Ordinal));
                    }
                    else
                    {
                        WriteFilesInDirectoryToChannel(directory, filesToProcessChannel, filter, new SortedSet<string>(StringComparer.Ordinal));
                    }
                }
                finally
                {
                    filesToProcessChannel.Writer.Complete();
                }
            });

            ChannelReader<string> reader = filesToProcessChannel.Reader;
            while (!reader.Completion.IsCompleted)
            {
                string currentFileToProcess;
                bool didTryReadFail;
                try
                {
                    this.cancellationToken.ThrowIfCancellationRequested();

                    reader.WaitToReadAsync(this.cancellationToken);

                    didTryReadFail = !reader.TryRead(out currentFileToProcess);
                }
                catch (Exception ex)
                {
                    this.isEnumerationEnded = true;

                    // WhenAll() waits on the task without triggering rethrow of any exceptions.
                    Task.WhenAll(directoryEnumerationTask);
                    if (directoryEnumerationTask.IsFaulted)
                    {
                        throw new AggregateException(ex, directoryEnumerationTask.Exception);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (didTryReadFail)
                {
                    // WaitToReadAsync can complete before TryRead can see the item.
                    // To work around, we loop back and try again.
                    continue;
                }

                yield return new EnumeratedArtifact(FileSystem)
                {
                    Uri = new Uri(currentFileToProcess, UriKind.Absolute),
                };
            }

            // If we finished enumeration because the worker thread died (and couldn't produce more files),
            // we will bubble out that Exception here:
            directoryEnumerationTask.Wait();

            // In event of cancellation worker thread returns out early w/o Exception.  On this thread that
            // could look like completing the directory traversal, so we check it again to be sure
            // before completing the enumeration.
            this.cancellationToken.ThrowIfCancellationRequested();
        }

        private void EnqueueAllFilesUnderDirectory(string directory, ChannelWriter<string> fileChannelWriter, string fileFilter, SortedSet<string> sortedDiskItemsBuffer)
        {
            if (CheckFaulted())
            {
                return;
            }

            WriteFilesInDirectoryToChannel(directory, fileChannelWriter, fileFilter, sortedDiskItemsBuffer);
            if (CheckFaulted())
            {
                return;
            }

            sortedDiskItemsBuffer.Clear();
            foreach (string childDirectory in FileSystem.DirectoryEnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                if (CheckFaulted())
                {
                    return;
                }

                sortedDiskItemsBuffer.Add(childDirectory);
            }

            foreach (string childDirectory in sortedDiskItemsBuffer.ToArray())
            {
                if (CheckFaulted())
                {
                    return;
                }

                // Skip subdirectories that are symbolic links to prevent infinite loops
                // and avoid unintentionally following symbolic links during recursion
                if (!FileSystem.IsSymbolicLink(childDirectory))
                {
                    EnqueueAllFilesUnderDirectory(childDirectory, fileChannelWriter, fileFilter, sortedDiskItemsBuffer);
                }
            }
        }

        private void WriteFilesInDirectoryToChannel(string directory, ChannelWriter<string> fileChannelWriter, string filter, SortedSet<string> sortedDiskItemsBuffer)
        {
            sortedDiskItemsBuffer.Clear();
            foreach (string childFile in FileSystem.DirectoryEnumerateFiles(directory, filter, SearchOption.TopDirectoryOnly))
            {
                string fullFilePath = Path.Combine(directory, childFile);
                sortedDiskItemsBuffer.Add(fullFilePath);
            }

            foreach (string childFile in sortedDiskItemsBuffer)
            {
                fileChannelWriter.WriteAsync(childFile).AsTask().Wait();
            }
        }

        private bool CheckFaulted()
        {
            return this.cancellationToken.IsCancellationRequested || this.isEnumerationEnded;
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
