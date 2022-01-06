// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    internal class NonDisposingDelegatingStream : Stream
    {
        private readonly Stream underlyingStream;

        /// <summary>
        /// This is a wrapper for a stream that protects the underlying stream from being disposed.
        /// Disposing this object is a no-op. To dispose the underlying stream use the DisposeUnderlyingStream method.
        /// </summary>
        /// <param name="underlyingStream"></param>
        internal NonDisposingDelegatingStream(Stream underlyingStream)
        {
            this.underlyingStream = underlyingStream ?? throw new ArgumentNullException(nameof(underlyingStream));
        }

        public override bool CanRead => underlyingStream.CanRead;

        public override bool CanSeek => underlyingStream.CanSeek;

        public override bool CanWrite => underlyingStream.CanWrite;

        public override long Length => underlyingStream.Length;

        public override long Position
        {
            set => throw new System.NotImplementedException();
            get => underlyingStream.Position;
        }

        public override void Flush() => underlyingStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => underlyingStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => underlyingStream.Seek(offset, origin);

        public override void SetLength(long value) => underlyingStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => underlyingStream.Write(buffer, offset, count);

        public void DisposeUnderlyingStream()
        {
            underlyingStream.Dispose();
        }
    }
}
