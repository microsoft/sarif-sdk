// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{

    public class ZipArchiveArtifact : IEnumeratedArtifact
    {
        private readonly HashSet<string> binaryExtensions;
        private readonly ZipArchive archive;
        private ZipArchiveEntry entry;
        private readonly Uri uri;
        private string contents;
        private byte[] bytes;

        public ZipArchiveArtifact(ZipArchive archive, ZipArchiveEntry entry, HashSet<string> binaryExtensions)
        {
            this.entry = entry;
            this.archive = archive;
            this.binaryExtensions = binaryExtensions;
            this.uri = new Uri(entry.FullName, UriKind.RelativeOrAbsolute);
        }

        public Uri Uri => this.uri;

        public Stream Stream
        {
            get
            {
                lock (this.archive)
                {
                    return entry != null
                        ? entry.Open()
                        : null;
                }
            }
            set => throw new NotImplementedException();
        }

        public Encoding Encoding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Contents
        {
            get => GetArtifactData().text;
            set => this.contents = value;
        }

        public byte[] Bytes
        {
            get => GetArtifactData().bytes;
            set => this.bytes = value;
        }

        private (string text, byte[] bytes) GetArtifactData()
        {
            if (this.contents == null && this.bytes == null)
            {
                lock (this.archive)
                {
                    if (this.contents == null && this.bytes == null)
                    {
                        string extension = Path.GetExtension(Uri.ToString());
                        if (this.binaryExtensions.Contains(extension))
                        {
                            this.bytes = new byte[Stream.Length];
                            Stream.Read(this.bytes, 0, this.bytes.Length);
                        }
                        else
                        {
                            this.contents = new StreamReader(Stream).ReadToEnd();
                        }
                    }
                }
                this.entry = null;
            }

            return (this.contents, this.bytes);
        }

        public long? SizeInBytes
        {
            get
            {
                lock (this.archive)
                {
                    if (this.entry != null)
                    {
                        return this.entry.Length;
                    }

                    return this.contents != null
                        ? this.contents.Length
                        : this.bytes.Length;
                }
            }
            set => throw new NotImplementedException();
        }
    }
}
