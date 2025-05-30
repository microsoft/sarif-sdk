// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    // TBD: this class should probably be a generic, with
    // EnumeratedArtifact<string> being a commonly utilized thing.
    public class EnumeratedArtifact(IFileSystem fileSystem) : IEnumeratedArtifact
    {
    private const int BinarySniffingHeaderSizeBytes = 1024;
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

    internal IFileSystem FileSystem { get; set; } = fileSystem;

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
            this.Stream = FileSystem.FileOpenRead(Uri.OriginalString);
        }

        RetrieveDataFromStream();

        this.Stream = null;

        return (this.contents, this.bytes);
    }

    private bool IsZipHeader(byte[] header, int length)
    {
        if (length < 4)
        {
            return false;
        }

        // ZIP files always begin with the 4-byte signature 'PK\x03\x04'
        // (0x50 0x4B 0x03 0x04) marking the start of a local file header.
        // If this signature is detected, treat the stream as binary (not
        // text) to avoid corrupting ZIP contents.
        return header[0] == (byte)'P' &&
                header[1] == (byte)'K' &&
                header[2] == 0x03 &&
                header[3] == 0x04;
    }

    private void RetrieveDataFromStream()
    {
        if (!this.Stream.CanSeek)
        {
            this.Stream = new PeekableStream(this.Stream, BinarySniffingHeaderSizeBytes);
        }

        byte[] header = new byte[BinarySniffingHeaderSizeBytes];
        int readLength = this.Stream.Read(header, 0, header.Length);

        bool isText = !IsZipHeader(header, readLength) && FileEncoding.IsTextualData(header, 0, readLength);

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
            if (this.Stream is PeekableStream peekable)
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
            }

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
                this.sizeInBytes = FileSystem.IsSymbolicLink(Uri.OriginalString)
                    ? (long)FileSystem.FileStreamLength(Uri.OriginalString)
                    : (long)FileSystem.FileInfoLength(Uri.OriginalString);
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
