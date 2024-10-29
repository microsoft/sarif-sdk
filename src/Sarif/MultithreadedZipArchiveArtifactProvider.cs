// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class MultithreadedZipArchiveArtifactProvider : ArtifactProvider
    {
        private readonly ZipArchive zipArchive;


        public MultithreadedZipArchiveArtifactProvider(ZipArchive zipArchive, IFileSystem fileSystem, Uri uri = null) : base(fileSystem, uri)
        {
            this.zipArchive = zipArchive;
        }

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

                    yield return new ZipArchiveArtifact(this.zipArchive, entry, ExtensionsDenyList, this.Uri);
                }
            }
        }
    }
}
