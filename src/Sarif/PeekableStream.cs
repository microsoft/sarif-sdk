// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// This stream provides a very limited seekability for Streams which would benefit from an ersatz Seek(Origin)
    /// capability, but which only need it at the beginning of a Stream.  Example use is: to categorize binary versus textual 
    /// data based on the beginning of a non-seekable Stream.
    /// </summary>
    internal class PeekableStream : Stream
    {
        private readonly Stream underlyingStream;
        private byte[] rewindBuffer;
        private int cursor;

        public PeekableStream(Stream underlyingStream, int initialPeekWindow)
        {
            this.underlyingStream = underlyingStream;
            this.rewindBuffer = new byte[initialPeekWindow];
            this.cursor = 0;
        }

        public override bool CanRead => true;

        // Rewind() is much weaker than Seek.  We should not advertise a capability we do not have.
        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => underlyingStream.Length;

        public override long Position 
        {
            get => Math.Min(underlyingStream.Position, cursor);
            set => throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.rewindBuffer == null)
            {
                // Common case: we're past the "peekability window" and are now just a thin wrapper around the underlying Stream.
                return underlyingStream.Read(buffer, offset, count);
            }
            else if (this.cursor == this.underlyingStream.Position)
            {
                // Buffer cursor is "caught up" to stream position, so we are not in a "rewound" state.
                if (this.cursor == this.rewindBuffer.Length)
                {
                    // A previous read has brought us to the brink of exceeding the "peekability window", and we are
                    // now reading more.  So we abandon the buffer, and serve the read as a thin wrapper.
                    this.rewindBuffer = null;
                    this.cursor = int.MaxValue;
                    return underlyingStream.Read(buffer, offset, count);
                }

                // Read into our rewind buffer for "backup" and then copy into caller buffer.
                // Buffer position is behind Stream position.  Service read out of the buffer.
                int aspirationalReadCount = Math.Min(count, (int)(this.rewindBuffer.Length - cursor));
                int actualReadCount = underlyingStream.Read(this.rewindBuffer, cursor, aspirationalReadCount);
                Array.Copy(this.rewindBuffer, cursor, buffer, offset, actualReadCount);

                cursor += actualReadCount;

                return actualReadCount;
            }
            else if (this.cursor <= this.underlyingStream.Position)
            {
                // We are rewound.  Service the read out of the rewind buffer.
                int safeReadCount = Math.Min(count, this.rewindBuffer.Length - cursor);
                safeReadCount = Math.Min(safeReadCount, (int)(this.underlyingStream.Position - cursor));
                Array.Copy(this.rewindBuffer, cursor, buffer, offset, safeReadCount);

                cursor += safeReadCount;
                return safeReadCount;
            }

            throw new InvalidOperationException($"Peekable stream in invalid state: cursor '{cursor}' pointing past stream position '{underlyingStream.Position}'.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void Rewind()
        {
            if (this.rewindBuffer == null)
            {
                throw new InvalidOperationException("This stream has read too many bytes and can not longer be rewound.");
            }

            this.cursor = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.underlyingStream.Dispose();
                this.rewindBuffer = null;
            }
        }
    }
}
