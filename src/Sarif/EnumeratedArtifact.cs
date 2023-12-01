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

        internal byte[] bytes;
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

        /// <summary>
        /// Gets or sets a property that determines whether the enumerated artifact
        /// can obtain data from a non-seekable stream. This operation will be
        /// less performant in any case where an artifact is a textual file (as
        /// the file will first be converted to a byte array to determine
        /// whether it is textual and subsequently turned into a string if it is).
        /// </summary>
        public bool SupportNonSeekableStreams { get; set; }

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
                Stream = FileSystem.FileOpenRead(Uri.LocalPath);
            }

            if (Stream.CanSeek)
            {
                RetrieveDataFromSeekableStream();
            }
            else
            {
                RetrieveDataFromNonSeekableStream();
            }

            Stream = null;

            return (this.contents, this.bytes);
        }

        private void RetrieveDataFromNonSeekableStream()
        {
            bool isText;
            if (!SupportNonSeekableStreams)
            {
                throw new InvalidOperationException("Stream is not seekable. Provide a seekable stream or set the 'SupportNonSeekableStreams' property.");
            }

            this.bytes = new byte[Stream.Length];
            int length = this.Stream.Read(this.bytes, 0, this.bytes.Length);
            isText = FileEncoding.CheckForTextualData(this.bytes, 0, length, out this.encoding);

            if (isText)
            {
                // If we have textual data and the encoding was null, we are UTF8
                // (which will be a perfectly valid encoding for ASCII as well).
                this.encoding ??= Encoding.UTF8;
                this.contents = encoding.GetString(this.bytes);
                this.bytes = null;
            }
        }

        private void RetrieveDataFromSeekableStream()
        {
            bool isText;

            // Reset to beginning of stream in case caller neglected to do so.
            this.Stream.Seek(0, SeekOrigin.Begin);

            byte[] header = new byte[1024];
            int length = this.Stream.Read(header, 0, header.Length);
            isText = FileEncoding.CheckForTextualData(header, 0, length, out this.encoding);

            if (isText)
            {
                // If we have textual data and the encoding was null, we are UTF8
                // (which will be a perfectly valid encoding for ASCII as well).
                this.encoding ??= Encoding.UTF8;
            }

            this.Stream.Seek(0, SeekOrigin.Begin);

            if (isText)
            {
                using var contentReader = new StreamReader(Stream);
                this.contents = contentReader.ReadToEnd();
            }
            else
            {
                this.bytes = new byte[Stream.Length];
                this.Stream.Read(this.bytes, 0, bytes.Length);
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
                else if (this.bytes != null)
                {
                    this.sizeInBytes = (int)this.bytes.Length;
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
