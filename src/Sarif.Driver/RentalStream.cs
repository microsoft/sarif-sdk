// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// "Rental" stream. Wraps a stream object, but prevents the client from calling dispose
    /// on that stream object. This allows a stream to be passed, for example to TextReader or
    /// TextWriter, without giving up ownership of the underlying stream.
    /// </summary>
    /// <seealso cref="System.IO.Stream"/>
    public sealed class RentalStream : Stream
    {
        /// <summary>The inner stream.</summary>
        private readonly Stream _innerStream;

        /// <summary>Initializes a new instance of the <see cref="RentalStream"/> class.</summary>
        /// <param name="sourceStream">The Stream to process.</param>
        public RentalStream(Stream sourceStream)
        {
            if (sourceStream == null)
            {
                throw new ArgumentNullException("sourceStream");
            }

            _innerStream = sourceStream;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream
        /// supports reading.
        /// </summary>
        /// <value>true if the stream supports reading; otherwise, false.</value>
        /// <seealso cref="System.IO.Stream.CanRead"/>
        public override bool CanRead
        {
            get
            {
                return _innerStream.CanRead;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream
        /// supports seeking.
        /// </summary>
        /// <value>true if the stream supports seeking; otherwise, false.</value>
        /// <seealso cref="System.IO.Stream.CanSeek"/>
        public override bool CanSeek
        {
            get
            {
                return _innerStream.CanSeek;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream
        /// supports writing.
        /// </summary>
        /// <value>true if the stream supports writing; otherwise, false.</value>
        /// <seealso cref="System.IO.Stream.CanWrite"/>
        public override bool CanWrite
        {
            get
            {
                return _innerStream.CanWrite;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <value>A long value representing the length of the stream in bytes.</value>
        /// <seealso cref="System.IO.Stream.Length"/>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not
        /// support seeking.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream
        /// was closed.</exception>
        public override long Length
        {
            get
            {
                return _innerStream.Length;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <value>The current position within the stream.</value>
        /// <seealso cref="System.IO.Stream.Position"/>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream
        /// was closed.</exception>
        public override long Position
        {
            get
            {
                return _innerStream.Position;
            }

            set
            {
                _innerStream.Position = value;
            }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any
        /// buffered data to be written to the underlying device.
        /// </summary>
        /// <seealso cref="System.IO.Stream.Flush()"/>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        public override void Flush()
        {
            _innerStream.Flush();
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and
        /// advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the
        /// specified byte array with the values between <paramref name="offset" /> and
        /// (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from
        /// the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to
        /// begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes
        /// requested if that many bytes are not currently available, or zero (0) if the end of the
        /// stream has been reached.
        /// </returns>
        /// <seealso cref="System.IO.Stream.Read(byte[],int,int)"/>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and
        /// <paramref name="count" /> is larger than the buffer length.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> or
        /// <paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream
        /// was closed.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the
        /// reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        /// <seealso cref="System.IO.Stream.Seek(long,SeekOrigin)"/>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such
        /// as if the stream is constructed from a pipe or console output.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream
        /// was closed.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        /// <summary>When overridden in a derived class, sets the length of the current stream.</summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <seealso cref="System.IO.Stream.SetLength(long)"/>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing
        /// and seeking, such as if the stream is constructed from a pipe or console output.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream
        /// was closed.</exception>
        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and
        /// advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes
        /// from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to
        /// begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <seealso cref="System.IO.Stream.Write(byte[],int,int)"/>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and
        /// <paramref name="count" /> is greater than the buffer length.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> or
        /// <paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream
        /// was closed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        /// <summary>Seeks the underlying stream back to the state it was in before this <see cref="RentalStream"/> was
        /// constructed. Satisfies the contract of the base class, System.IO.Stream.Dispose(bool).</summary>
        /// <param name="disposing">Unused parameter.</param>
        /// <seealso cref="System.IO.Stream.Dispose(bool)"/>
        // Justification: This would sort of defeat the purpose of this class :)
        [SuppressMessage("Microsoft.Usage", "CA2215:Dispose methods should call base class dispose")]
        protected override void Dispose(bool disposing)
        {
            // Purposely does nothing -- the underlying stream should not receive this call....
        }
    }
}
