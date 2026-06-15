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
        private byte[] rawBytes;
        private string contents;
        private bool isTextual;
        private bool resolved;

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
                return !this.isTextual;
            }
        }

        public Stream Stream
        {
            get
            {
                GetArtifactData();

                return this.rawBytes == null
                    ? null
                    : new MemoryStream(this.rawBytes, writable: false);
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
            get
            {
                GetArtifactData();
                return this.isTextual ? this.contents : null;
            }
            set => throw new NotImplementedException();
        }

        public byte[] Bytes
        {
            get
            {
                GetArtifactData();
                return this.isTextual ? null : this.rawBytes;
            }
            set => throw new NotImplementedException();
        }

        private void GetArtifactData()
        {
            if (this.resolved)
            {
                return;
            }

            lock (this.archive)
            {
                if (!this.resolved && this.entry != null)
                {
                    const int PeekWindowBytes = 1024;

                    byte[] raw = ReadAllBytes(this.entry);
                    this.rawBytes = raw;

                    int peekLength = Math.Min(raw.Length, PeekWindowBytes);
                    this.isTextual = FileEncoding.IsTextualData(raw, 0, peekLength);

                    if (this.isTextual)
                    {
                        using var reader = new StreamReader(new MemoryStream(raw, writable: false));
                        this.contents = reader.ReadToEnd();
                    }

                    this.entry = null;
                    this.resolved = true;
                }
            }
        }

        private static byte[] ReadAllBytes(ZipArchiveEntry entry)
        {
            using Stream entryStream = entry.Open();

            int capacity = entry.Length > 0 && entry.Length <= int.MaxValue
                ? (int)entry.Length
                : 0;

            using var ms = new MemoryStream(capacity);
            entryStream.CopyTo(ms);

            byte[] buffer = ms.GetBuffer();
            return buffer.Length == ms.Length ? buffer : ms.ToArray();
        }

        public long? SizeInBytes
        {
            get
            {
                if (this.resolved)
                {
                    return this.isTextual ? this.contents.Length : this.rawBytes.Length;
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
