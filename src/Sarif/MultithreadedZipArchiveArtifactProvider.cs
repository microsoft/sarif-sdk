// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class MultithreadedZipArchiveArtifactProvider : ArtifactProvider
    {
        private readonly ZipArchive zipArchive;

        public MultithreadedZipArchiveArtifactProvider(ZipArchive zipArchive, IFileSystem fileSystem) : base(fileSystem)
        {
            this.zipArchive = zipArchive;

            this.binaryExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


            this.binaryExtensions.Add(".bmp");
            this.binaryExtensions.Add(".cab");
            this.binaryExtensions.Add(".cer");
            this.binaryExtensions.Add(".der");
            this.binaryExtensions.Add(".dll");
            this.binaryExtensions.Add(".exe");
            this.binaryExtensions.Add(".gif");
            this.binaryExtensions.Add(".gz");
            this.binaryExtensions.Add(".iso");
            this.binaryExtensions.Add(".jpe");
            this.binaryExtensions.Add(".jpeg");
            this.binaryExtensions.Add(".lock");
            this.binaryExtensions.Add(".p12");
            this.binaryExtensions.Add(".pack");
            this.binaryExtensions.Add(".pfx");
            this.binaryExtensions.Add(".pkcs12");
            this.binaryExtensions.Add(".png");
            this.binaryExtensions.Add(".psd");
            this.binaryExtensions.Add(".rar");
            this.binaryExtensions.Add(".tar");
            this.binaryExtensions.Add(".tif");
            this.binaryExtensions.Add(".tiff");
            this.binaryExtensions.Add(".xcf");
            this.binaryExtensions.Add(".zip");
        }

        private readonly HashSet<string> binaryExtensions;

        public override IEnumerable<IEnumeratedArtifact> Artifacts
        {
            get
            {
                foreach (ZipArchiveEntry entry in this.zipArchive.Entries)
                {
                    yield return new ZipArchiveArtifact(this.zipArchive, entry, this.binaryExtensions);
                }
            }
        }
    }

}
