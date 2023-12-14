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

        private bool isBinary;
        internal Lazy<byte[]> bytes;
        internal Lazy<string> contents;

        private Encoding encoding;

        public Uri Uri { get; set; }

        public bool IsBinary
        {
            get
            {
                GetArtifactData();
                return this.isBinary;
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
            get
            {
                GetArtifactData();

                if (!this.isBinary && this.bytes != null)
                {
                    // If we have textual data and the encoding was null, we are UTF8
                    // (which will be a perfectly valid encoding for ASCII as well).
                    this.encoding ??= Encoding.UTF8;
                    this.contents = new Lazy<string>(()=> { return encoding.GetString(this.bytes.Value); });
                    this.bytes = null;
                }

                return this.contents.Value;
            }

            set
            {
                this.contents = new Lazy<string>(() => value);
                _ = this.contents.Value;
            }
        }

        public byte[] Bytes
        {
            get
            {
                GetArtifactData();

                if (!this.isBinary && this.bytes.Value != null)
                {
                    // If we have textual data and the encoding was null, we are UTF8
                    // (which will be a perfectly valid encoding for ASCII as well).
                    this.encoding ??= Encoding.UTF8;
                    this.contents = new Lazy<string>(() => { return encoding.GetString(this.bytes.Value); });
                    this.bytes = null;
                    return null;
                }

                return this.bytes.Value;
            }
            set
            {
                this.bytes = new Lazy<byte[]>(() => value);
                _ = this.bytes.Value;
            }
        }

        private (Lazy<string> text, Lazy<byte[]> bytes) GetArtifactData()
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
            this.bytes = new Lazy<byte[]>(() =>
            {
                byte[] bytes = new byte[Stream.Length];
                int length = this.Stream.Read(bytes, 0, bytes.Length);
                this.isBinary = !FileEncoding.IsTextualData(bytes, 0, length);
                return bytes;
            });
        }

        private void RetrieveDataFromSeekableStream()
        {
            // Reset to beginning of stream in case caller neglected to do so.
            this.Stream.Seek(0, SeekOrigin.Begin);

            byte[] header = new byte[1024];
            int length = this.Stream.Read(header, 0, header.Length);
            this.isBinary = !FileEncoding.IsTextualData(header, 0, length);

            this.Stream.Seek(0, SeekOrigin.Begin);

            if (!this.isBinary)
            {
                using var contentReader = new StreamReader(Stream);
                this.contents = new Lazy<string>(()=> contentReader.ReadToEnd());
            }
            else
            {
                this.bytes = new Lazy<byte[]>(() => 
                {
                    byte[] bytes = new byte[Stream.Length];
                    this.Stream.Read(bytes, 0, bytes.Length);
                    return bytes;
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

                if (this.contents?.IsValueCreated == true && this.contents.Value != null)
                {
                    this.sizeInBytes = (long)this.contents.Value.Length;
                }
                else if (this.bytes?.IsValueCreated == true && this.bytes.Value != null)
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

                return this.sizeInBytes;
            }
            set
            {
                this.sizeInBytes = value;
            }
        }
    }
}
