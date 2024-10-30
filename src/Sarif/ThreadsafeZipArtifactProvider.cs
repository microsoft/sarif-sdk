// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A thread-safe implementation of <see cref="ArtifactProvider"/> that can be used to
    /// enumerate scan targets from a zip archive. The class is thread-safe in that the
    /// containing archive instance is passed to the each constituent archive element.
    /// This object is used to synchronize decompressing the archive elements (which
    /// itself is not a thread-safe operation).
    /// </summary>
    public class ThreadsafeZipArtifactProvider : ArtifactProvider
    {
        private readonly ZipArchive zipArchive;

        public ThreadsafeZipArtifactProvider(ZipArchive zipArchive, IFileSystem fileSystem, Uri uri = null) : base(fileSystem, uri)
        {
            this.zipArchive = zipArchive;
            this.FileSystem = fileSystem;
        }

        private static readonly ISet<string> s_extensionsDenyList =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                    ".bmp",
                    ".cab",
                    ".cer",
                    ".der",
                    ".dll",
                    ".exe",
                    ".gif",
                    ".gz",
                    ".iso",
                    ".jpe",
                    ".jpeg",
                    ".lock",
                    ".p12",
                    ".pack",
                    ".pfx",
                    ".pkcs12",
                    ".png",
                    ".psd",
                    ".rar",
                    ".tar",
                    ".tif",
                    ".tiff",
                    ".xcf",
                    "."
            };

        public override IEnumerable<IEnumeratedArtifact> Artifacts
        {
            get
            {
                System.Collections.ObjectModel.ReadOnlyCollection<ZipArchiveEntry> entries = null;
                try
                {
                    entries = this.zipArchive.Entries;
                }
                catch (InvalidDataException ex)
                {
                    UnhandledException = ex;
                }

                if (entries == null) { yield break; }

                foreach (ZipArchiveEntry entry in entries)
                {
                    if (entry.FullName.EndsWith("/") && entry.CompressedLength == 0)
                    {
                        // We have a directory entry. We don't want to return these,
                        // as they can't be analyzed and will simply generate 
                        // 'zero-byte file encountered' warnings.
                        continue;
                    }

                    yield return new ZipArchiveArtifact(this.zipArchive, entry, s_extensionsDenyList, this.Uri);
                }
            }
        }
    }
}
