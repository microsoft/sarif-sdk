// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class DelegatingStreamTests
    {
        [Fact]
        public void DelegatingStreamBasicRead()
        {
            const string testStr = "Hello";
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testStr));
            DelegatingStream delegatingStream = new DelegatingStream(memoryStream);

            Assert.Equal(0, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
            using StreamReader streamReader = new StreamReader(delegatingStream);
            Assert.Equal(testStr, streamReader.ReadToEnd());
            Assert.Equal(testStr.Length, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void DelegatingStreamBasicSeek()
        {
            const string testStr = "Hello";
            const int newPosition = 3;
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testStr));
            DelegatingStream delegatingStream = new DelegatingStream(memoryStream);

            Assert.Equal(0, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);

            delegatingStream.Position = newPosition;

            Assert.Equal(newPosition, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void DelegatingStreamDontPerturbPositionOnCtor()
        {
            const string testStr = "Hello";
            const int startingPosition = 3;
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testStr));
            memoryStream.Position = startingPosition;
            DelegatingStream delegatingStream = new DelegatingStream(memoryStream);

            Assert.Equal(startingPosition, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void DelegatingStreamNonSeekable()
        {
            const string testStr = "Hello";
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testStr));
            NonSeekableStream nonSeekableStream = new NonSeekableStream(memoryStream);
            DelegatingStream delegatingStream = new DelegatingStream(nonSeekableStream);

            using StreamReader streamReader = new StreamReader(delegatingStream);
            Assert.Equal(testStr, streamReader.ReadToEnd());
        }

        private class NonSeekableStream : Stream
        {
            private readonly Stream stream;

            /// <summary>
            /// This is a wrapper for a that prevents seeking for tests.
            /// </summary>
            /// <param name="underlyingStream"></param>
            internal NonSeekableStream(Stream underlyingStream)
            {
                stream = underlyingStream;
            }

            public override bool CanRead => stream.CanRead;

            public override bool CanSeek => false;

            public override bool CanWrite => stream.CanWrite;

            public override long Length => stream.Length;

            public override long Position
            {
                set
                {
                    throw new NotSupportedException();
                }
                get => stream.Position;
            }

            public override void Flush() => stream.Flush();

            public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => stream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    stream.Dispose();
                }
                base.Dispose(disposing);
            }
        }

    }
}
