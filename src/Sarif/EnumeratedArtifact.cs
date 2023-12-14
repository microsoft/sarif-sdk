// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    // TBD: this class should probably be a generic, with
    // EnumeratedArtifact<string> being a commonly utilized thing.
    public class EnumeratedArtifact : IEnumeratedArtifact
    {
        public EnumeratedArtifact(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        internal Lazy<byte[]> bytes;
        internal string contents;

        private Encoding encoding;

        public Uri Uri { get; set; }

        public bool IsBinary
        {
            get
            {
                GetArtifactData();
                return this.contents == null;
            }
        }

        public Stream Stream { get; set; }

        public Encoding Encoding
        {
            get
            {
                if (encoding == null)
                {
                    GetArtifactData();
                }
                return this.encoding;
            }

            set => this.encoding = value;
        }

        internal IFileSystem FileSystem { get; set; }

        public string Contents
        {
            get => GetArtifactData().text;
            set => this.contents = value;
        }

        public Lazy<byte[]> Bytes
        {
            get => GetArtifactData().bytes;
            set => this.bytes = value;
        }

        private (string text, Lazy<byte[]> bytes) GetArtifactData()
        {
            if (this.contents != null)
            {
                return (this.contents, bytes: null);
            }

            if (this.bytes != null)
            {
                return (text: null, this.bytes);
            }

            if (Stream == null && this.contents == null && this.bytes == null)
            {
                if (Uri == null ||
                    !Uri.IsAbsoluteUri ||
                    (Uri.IsAbsoluteUri && !Uri.IsFile))
                {
                    throw new InvalidOperationException("An absolute URI pointing to a file location was not available.");
                }

                // This is our client-side, disk-based file retrieval case.
                this.Stream = FileSystem.FileOpenRead(Uri.LocalPath);
            }

            if (Stream.CanSeek)
            {
                RetrieveDataFromSeekableStream();
            }
            else
            {
                RetrieveDataFromNonSeekableStream();
            }

            this.Stream = null;

            return (this.contents, this.bytes);
        }

        private void RetrieveDataFromNonSeekableStream()
        {
            bool isText;

            byte[] buffer = new byte[Stream.Length];
            int length = this.Stream.Read(buffer, 0, buffer.Length);
            isText = FileEncoding.IsTextualData(buffer, 0, length);

            if (isText)
            {
                // If we have textual data and the encoding was null, we are UTF8
                // (which will be a perfectly valid encoding for ASCII as well).
                this.encoding ??= Encoding.UTF8;
                this.contents = encoding.GetString(buffer);
            }
            else
            {
                // This wasn't very lazy, but for seekable streams we can't resett the stream so we needed to
                // read and retain the whole thing in the case of a byte[] return value.
                this.bytes = new Lazy<byte[]>(() => buffer);
            }
        }

        private void RetrieveDataFromSeekableStream()
        {
            bool isText;

            // Reset to beginning of stream in case caller neglected to do so.
            this.Stream.Seek(0, SeekOrigin.Begin);

            byte[] header = new byte[1024];
            int length = this.Stream.Read(header, 0, header.Length);
            isText = FileEncoding.IsTextualData(header, 0, length);

            this.Stream.Seek(0, SeekOrigin.Begin);

            if (isText)
            {
                using var contentReader = new StreamReader(Stream);
                this.contents = contentReader.ReadToEnd();
            }
            else
            {
                this.bytes = new Lazy<byte[]>(() =>
                {
                    byte[] result = new byte[Stream.Length];
                    this.Stream.Read(result, 0, result.Length);
                    return result;
                });
            }
        }

        public long? sizeInBytes;

        public long? SizeInBytes
        {
            get
            {
                if (this.sizeInBytes != null)
                {
                    return this.sizeInBytes.Value;
                };

                if (this.contents != null)
                {
                    this.sizeInBytes = (long)this.contents.Length;
                }
                else if (this.bytes != null && this.bytes.IsValueCreated)
                {
                    this.sizeInBytes = (int)this.bytes.Value.Length;
                }
                else if (this.Stream != null)
                {
                    this.SizeInBytes = (long)this.Stream.Length;
                }
                else if (Uri != null && Uri.IsAbsoluteUri && Uri.IsFile)
                {
                    this.sizeInBytes = (long)FileSystem.FileInfoLength(Uri.LocalPath);
                }
                else if (this.Contents != null)
                {
                    this.SizeInBytes = (long)this.Contents.Length;
                }
                else if (this.bytes != null)
                {
                    this.sizeInBytes = (int)this.bytes.Value.Length;
                }

                return this.sizeInBytes;
            }
            set
            {
                this.sizeInBytes = value;
            }
        }
    }
}
