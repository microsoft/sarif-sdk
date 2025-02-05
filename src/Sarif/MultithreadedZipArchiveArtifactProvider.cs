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
        private static readonly byte[] ZipSignature = { 0x50, 0x4B, 0x03, 0x04 };
        private readonly ZipArchive zipArchive;
        private readonly Uri uri;

        public ISet<string> BinaryFileExtensions { get; set; } = new StringSet();

        public ISet<string> OpcFileExtensions { get; set; } = new StringSet();


        public MultithreadedZipArchiveArtifactProvider(Uri uri, ZipArchive zipArchive, IFileSystem fileSystem) : base(fileSystem)
        {
            this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
            this.zipArchive = zipArchive ?? throw new ArgumentNullException(nameof(zipArchive));
        }

        internal static bool IsOpenPackagingConventionsFile(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[ZipSignature.Length];
                int bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                if (bytesRead < ZipSignature.Length)
                {
                    return false;
                }

                for (int i = 0; i < ZipSignature.Length; i++)
                {
                    if (buffer[i] != ZipSignature[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override IEnumerable<IEnumeratedArtifact> Artifacts
        {
            get
            {
                foreach (ZipArchiveEntry entry in this.zipArchive.Entries)
                {
                    if (entry.FullName.EndsWith("/")) { continue; }
                    yield return new ZipArchiveArtifact(this.uri, this.zipArchive, entry, BinaryFileExtensions);
                }
            }
        }
    }

}
