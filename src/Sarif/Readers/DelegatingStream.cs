// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    internal class DelegatingStream : Stream
    {
        private readonly Stream stream;

        /// <summary>
        /// This is a wrapper for a stream that protects the underlying stream from being disposed.
        /// Disposing this object is a no-op. To dispose the underlying stream use the DisposeUnderlyingStream method.
        /// </summary>
        /// <param name="underlyingStream"></param>
        internal DelegatingStream(Stream underlyingStream)
        {
            stream = underlyingStream;
        }

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length;

        public override long Position
        {
            set
            {
                stream.Position = value;
            }
            get => stream.Position;
        }

        public override void Flush() => stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);

        public override void SetLength(long value) => stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);

        /// <summary>
        /// Calls the dispose method on the wrapped stream.
        /// </summary>
        public void DisposeUnderlyingStream()
        {
            stream.Dispose();
        }
    }
}
