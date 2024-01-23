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
        private const int BinarySniffingHeaderSizeBytes = 1024;

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
                this.Stream = FileSystem.FileOpenRead(Uri.LocalPath);
            }

            RetrieveDataFromStream(this);

            this.Stream = null;

            return (this.contents, this.bytes);
        }

        internal static void RetrieveDataFromStream(IEnumeratedArtifact artifact)
        {
            if (!artifact.Stream.CanSeek)
            {
                artifact.Stream = new PeekableStream(artifact.Stream, BinarySniffingHeaderSizeBytes);
            }

            byte[] header = new byte[BinarySniffingHeaderSizeBytes];
            int length = artifact.Stream.Read(header, 0, header.Length);
            bool isText = FileEncoding.IsTextualData(header, 0, length);

            TryRewindStream(artifact);

            if (isText)
            {
                using var contentReader = new StreamReader(artifact.Stream);
                artifact.Contents = contentReader.ReadToEnd();
            }
            else
            {
                long? artifactSizeHint = artifact.SizeInBytes.Value;
                byte[] buffer = new byte[artifactSizeHint ?? 2 * 1024];

                var memoryStream = new MemoryStream(buffer);
                artifact.Stream.CopyTo(memoryStream);

                byte[] memStreamBuffer = memoryStream.GetBuffer();
                if (memStreamBuffer.Length == memoryStream.Length)
                {
                    // The goal with the hint is to get a memory stream whose underlying buffer is sized exactly correctly.
                    artifact.Bytes = memStreamBuffer;
                }
                else
                {
                    // If the hint failed us, we have to take a copy to align the buffer.
                    byte[] newBuffer = new byte[memoryStream.Length];
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.Read(newBuffer, 0, (int)(memoryStream.Length));
                    artifact.Bytes = newBuffer;
                }
            }
        }

        private static void TryRewindStream(IEnumeratedArtifact artifact)
        {
            if (artifact.Stream.CanSeek)
            {
                artifact.Stream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                var peekable = artifact.Stream as PeekableStream;
                if (peekable != null)
                {
                    peekable.Rewind();
                }
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
