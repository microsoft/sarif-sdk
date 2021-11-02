// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Readers
{
    internal class NonSeekableStream : Stream
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
