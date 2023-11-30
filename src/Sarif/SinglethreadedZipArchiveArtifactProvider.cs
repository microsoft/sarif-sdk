// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SinglethreadedZipArchiveArtifactProvider : ArtifactProvider
    {
        public SinglethreadedZipArchiveArtifactProvider(ZipArchive zipArchive, IFileSystem fileSystem) : base(fileSystem)
        {
            var artifacts = new List<IEnumeratedArtifact>();

            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                artifacts.Add(new EnumeratedArtifact(Sarif.FileSystem.Instance)
                {
                    Uri = new Uri(entry.FullName, UriKind.RelativeOrAbsolute),
                    Contents = new StreamReader(entry.Open()).ReadToEnd()
                });
            }

            Artifacts = artifacts;
        }
    }
}
