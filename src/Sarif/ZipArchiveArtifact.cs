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
        private byte[] bytes;

        public ZipArchiveArtifact(Uri archiveUri,
                                  ZipArchive archive,
                                  ZipArchiveEntry entry,
                                  ISet<string> binaryExtensions = null)
        {
            this.entry = entry ?? throw new ArgumentNullException(nameof(entry));
            this.archive = archive ?? throw new ArgumentNullException(nameof(archive));

            this.binaryExtensions = binaryExtensions ?? new HashSet<string>();
            this.uri = archiveUri != null
                ? new Uri($"{archiveUri}?path={entry.FullName}", UriKind.RelativeOrAbsolute)
                : new Uri(entry.FullName, UriKind.Relative);
        }

        public Uri Uri => this.uri;

        public bool IsBinary
        {
            get
            {
                GetArtifactData();
                return this.bytes != null;
            }
        }

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

        public byte[] Bytes
        {
            get => GetArtifactData().bytes;
            set => throw new NotImplementedException();
        }

        private (string text, byte[] bytes) GetArtifactData()
        {
            if (this.contents == null && this.bytes == null)
            {
                lock (this.archive)
                {
                    if (this.contents == null && this.bytes == null)
                    {
                        const int PeekWindowBytes = 1024;
                        var peekable = new PeekableStream(this.Stream, PeekWindowBytes);

                        byte[] header = new byte[PeekWindowBytes];
                        int readLength = this.Stream.Read(header, 0, header.Length);

                        bool isText = FileEncoding.IsTextualData(header, 0, readLength);

                        peekable.Rewind();

                        if (isText)
                        {
                            this.contents = new StreamReader(Stream).ReadToEnd();
                        }
                        else
                        {
                            // The underlying System.IO.Compression.DeflateStream throws on reads to get_Length.
                            using var ms = new MemoryStream((int)SizeInBytes.Value);
                            this.Stream.CopyTo(ms);

                            byte[] memStreamBuffer = ms.GetBuffer();
                            if (memStreamBuffer.Length == ms.Position)
                            {
                                // We might have succeeded in exactly sizing the MemoryStream.  In that case, we can just use it.
                                this.bytes = memStreamBuffer;
                            }
                            else
                            {
                                // No luck.  Have to take a copy to align the buffers.
                                ms.Position = 0;
                                this.bytes = new byte[ms.Length];

                                ms.Read(this.bytes, 0, this.bytes.Length);
                            }
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
                if (this.contents != null)
                {
                    return this.contents.Length;
                }

                if (this.bytes != null)
                {
                    return this.bytes.Length;
                }

                lock (this.archive)
                {
                    if (this.entry != null)
                    {
                        return this.entry.Length;
                    }
                }

                return null;
            }

            set => throw new NotImplementedException();
        }
    }
}
