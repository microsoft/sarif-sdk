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
        private readonly ISet<string> binaryExtensions;
        private readonly ZipArchive archive;
        private ZipArchiveEntry entry;
        private readonly Uri uri;
        private string contents;
        private Lazy<byte[]> bytes;

        public ZipArchiveArtifact(ZipArchive archive, ZipArchiveEntry entry, ISet<string> binaryExtensions = null)
        {
            this.entry = entry ?? throw new ArgumentNullException(nameof(entry));
            this.archive = archive ?? throw new ArgumentNullException(nameof(archive));

            this.binaryExtensions = binaryExtensions ?? new HashSet<string>();
            this.uri = new Uri(entry.FullName, UriKind.RelativeOrAbsolute);
        }

        public Uri Uri => this.uri;

        public Stream Stream
        {
            get
            {
                if (this.entry == null)
                {
                    return null;
                }

                lock (this.archive)
                {
                    return entry.Open();
                }
            }
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Raises NotImplementedException as we can't retrieve or set encoding
        /// currently. Assessing this data requires decompressing the archive
        /// stream. Currently, our encoding detection isn't highly developed.
        /// In the future, we should consider eliminating the Encoding property
        /// entirely from IEnumeratedArtifact or do the work of handling the
        /// range of text encodings.

        /// </summary>
        public Encoding Encoding
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string Contents
        {
            get => GetArtifactData().text;
            set => throw new NotImplementedException();
        }

        public Lazy<byte[]> Bytes
        {
            get => GetArtifactData().bytes;
            set => throw new NotImplementedException();
        }

        private (string text, Lazy<byte[]> bytes) GetArtifactData()
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
                            byte[] buffer = new byte[Stream.Length];
                            Stream.Read(buffer, 0, buffer.Length);
                            this.bytes = new Lazy<byte[]>(() => buffer);
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
                        : this.bytes.Value.Length;
                }
            }
            set => throw new NotImplementedException();
        }
    }
}
