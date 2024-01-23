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
        private const int BinarySniffingHeaderSizeBytes = 1024;

        private readonly ISet<string> binaryExtensions;
        private readonly ZipArchive archive;
        private ZipArchiveEntry entry;
        private Stream entryStream;
        private readonly Uri uri;
        private string contents;
        private byte[] bytes;

        public ZipArchiveArtifact(ZipArchive archive, ZipArchiveEntry entry, ISet<string> binaryExtensions = null)
        {
            this.entry = entry ?? throw new ArgumentNullException(nameof(entry));
            this.archive = archive ?? throw new ArgumentNullException(nameof(archive));

            this.binaryExtensions = binaryExtensions ?? new HashSet<string>();
            this.uri = new Uri(entry.FullName, UriKind.RelativeOrAbsolute);
        }

        public Uri Uri => this.uri;

        public bool IsBinary
        {
            get
            {
                string extension = Path.GetExtension(Uri.ToString());
                return this.binaryExtensions.Contains(extension);
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
                    this.entryStream = entry.Open();
                    return this.entryStream;
                }
            }
            set => this.entryStream = value;
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
            if (this.contents != null)
            {
                return (this.contents, bytes: null);
            }

            if (this.bytes != null)
            {
                return (text: null, this.bytes);
            }

            if (this.entryStream == null && this.contents == null && this.bytes == null)
            {
                lock (this.archive)
                {
                    // This is our client-side, disk-based file retrieval case.
                    RetrieveDataFromStream();
                }
            }

            this.Stream = null;
            this.entry = null;

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

        private void RetrieveDataFromStream()
        {
            if (!this.Stream.CanSeek)
            {
                this.Stream = new PeekableStream(this.Stream, BinarySniffingHeaderSizeBytes);
            }

            byte[] header = new byte[BinarySniffingHeaderSizeBytes];
            int length = this.Stream.Read(header, 0, header.Length);
            bool isText = FileEncoding.IsTextualData(header, 0, length);

            TryRewindStream();

            if (isText)
            {
                using var contentReader = new StreamReader(Stream);
                this.contents = contentReader.ReadToEnd();
            }
            else
            {
                this.bytes = new byte[Stream.Length];
                var memoryStream = new MemoryStream(this.bytes);
                this.Stream.CopyTo(memoryStream);
            }
        }

        private void TryRewindStream()
        {
            if (this.Stream.CanSeek)
            {
                this.Stream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                var peekable = this.Stream as PeekableStream;
                if (peekable != null)
                {
                    peekable.Rewind();
                }
            }
        }
    }
}
