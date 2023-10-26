// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

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

            var filesToProcessChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { AllowSynchronousContinuations = true, SingleReader = true, SingleWriter = true });

            Task directoryEnumerationTask;
            if (this.recurse)
            {
                directoryEnumerationTask = Task.Run(() =>
                {
                    EnqueueAllDirectories(directory, filesToProcessChannel.Writer, filter);
                    filesToProcessChannel.Writer.Complete();
                });
            }
            else
            {
                var sortedDiskItems = new SortedSet<string>();
                foreach (string file in FileSystem.DirectoryEnumerateFiles(directory, filter, SearchOption.TopDirectoryOnly))
                {
                    sortedDiskItems.Add(Path.Combine(directory, file));
                }

                foreach (string file in sortedDiskItems)
                {
                    filesToProcessChannel.Writer.TryWrite(file);
                }

                filesToProcessChannel.Writer.Complete();
                directoryEnumerationTask = Task.CompletedTask;
            }

            bool didEnumerationFinishWithoutFaults = false;
            try
            {
                ChannelReader<string> reader = filesToProcessChannel.Reader;
                while (!reader.Completion.IsCompleted)
                {
                    reader.WaitToReadAsync(this.cancellationToken);

                    string currentFileToProcess;
                    if (!reader.TryRead(out currentFileToProcess))
                    {
                        // WaitToReadAsync can complete before TryRead can see the item.
                        continue;
                    }

                    yield return new EnumeratedArtifact(FileSystem)
                    {
                        Uri = new Uri(currentFileToProcess, UriKind.Absolute),
                    };
                }

                didEnumerationFinishWithoutFaults = true;
            }
            finally
            {
                // We can't catch exceptions thrown from a block that yields, but we can run a defensive finally block.
                // Start by setting a flag that lets the worker thread know to shut down.
                this.isEnumerationEnded = true;

                if (didEnumerationFinishWithoutFaults)
                {
                    // We are not executing a finally block that is running due to an Exception.
                    // If any exceptions were triggered on the worker thread, we want to see them.  This will allow them to bubble out.
                    directoryEnumerationTask.Wait();
                }
                else
                {
                    // The loop didn't complete, so we're running in an Exception context.  Wait for the worker to complete,
                    // but don't trigger rethrowing of any Exceptions on this thread.
                    Task.WhenAll(directoryEnumerationTask).Wait();
                }
            }
        }

        private void EnqueueAllDirectories(string directory, ChannelWriter<string> fileChannelWriter, string filter)
        {
            if (CheckFaulted())
            {
                return;
            }

            var sortedDiskItems = new SortedSet<string>();
            foreach (string childFile in FileSystem.DirectoryEnumerateFiles(directory, filter, SearchOption.TopDirectoryOnly))
            {
                if (CheckFaulted())
                {
                    return;
                }

                string fullFilePath = Path.Combine(directory, childFile);
                sortedDiskItems.Add(fullFilePath);
            }

            foreach (string childFile in sortedDiskItems)
            {
                if (CheckFaulted())
                {
                    return;
                }

                fileChannelWriter.WriteAsync(childFile).AsTask().Wait();
            }

            sortedDiskItems.Clear();
            foreach (string childDirectory in FileSystem.DirectoryEnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                if (CheckFaulted())
                {
                    return;
                }

                sortedDiskItems.Add(childDirectory);
            }

            foreach (string childDirectory in sortedDiskItems)
            {
                if (CheckFaulted())
                {
                    return;
                }

                EnqueueAllDirectories(childDirectory, fileChannelWriter, filter);
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
