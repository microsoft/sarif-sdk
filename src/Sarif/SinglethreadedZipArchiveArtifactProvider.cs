// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SinglethreadedZipArchiveArtifactProvider : ArtifactProvider
    {
        public SinglethreadedZipArchiveArtifactProvider(Uri uri, ZipArchive zipArchive, IFileSystem fileSystem) : base(fileSystem)
        {
            var artifacts = new List<IEnumeratedArtifact>();

            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                if (entry.FullName.EndsWith("/")) { continue; }

                var artifact = new EnumeratedArtifact(Sarif.FileSystem.Instance)
                {
                    Uri = new Uri($"{uri}?{entry.FullName}"),
                    Stream = entry.Open(),
                };

                // This step will fault in all artifact contents, whether textual
                // or binary, and clear the zip stream, which is the point.
                artifact.Contents = artifact.Contents;

                artifacts.Add(artifact);
            }

            Artifacts = artifacts;
        }
    }
}
