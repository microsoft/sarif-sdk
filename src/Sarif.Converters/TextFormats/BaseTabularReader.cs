// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Converters.TextFormats
{
    /// <summary>
    ///  Base class for tabular file readers.
    ///  This class provides buffer management (refill and resize).
    /// </summary>
    internal abstract class BaseTabularReader : IDisposable
    {
        public const int DefaultBufferSize = 16 * 1024;

        private StreamReader _reader;

        protected char[] _buffer;
        protected int _bufferFilledLength;
        protected int _nextIndex;

        protected List<string> _currentRow;

        public int RowCountRead { get; protected set; }

        public BaseTabularReader(Stream stream, int bufferSize = DefaultBufferSize)
        {
            _reader = new StreamReader(stream);

            _buffer = new char[bufferSize];
            _bufferFilledLength = 0;
            _nextIndex = 0;

            _currentRow = new List<string>();
        }

        /// <summary>
        ///  Read the next row from the file.
        /// </summary>
        /// <remarks>
        ///  Implementers must find the cells and end of the next row,
        ///  filling _currentRow with the new cell values.
        ///  
        ///  See TsvReader for a simple but full implementation.
        /// </remarks>
        /// <returns>True if another row was found, False otherwise</returns>
        public abstract bool NextRow();

        /// <summary>
        ///  Read more from the file into the buffer.
        /// </summary>
        /// <returns>True if more was read, False if end of file was previously reached.</returns>
        protected bool RefillBuffer()
        {
            // If there is nothing else, return false to say there's no more
            // RefillBuffer must not shift the buffer if it won't read any more
            if (_reader.EndOfStream) { return false; }

            if (_nextIndex > 0)
            {
                int unusedLength = _bufferFilledLength - _nextIndex;
                if (unusedLength > 0)
                {
                    // If there's remaining unused content, copy it to the start of the buffer
                    Buffer.BlockCopy(_buffer, 2 * _nextIndex, _buffer, 0, 2 * unusedLength);

                    // Next character is now at start of buffer
                    _nextIndex = 0;
                    _bufferFilledLength = unusedLength;
                }
                else
                {
                    // If we consumed everything (or more), we'll refill the whole buffer, pre-ignoring any characters in excess of the buffer already consumed. 
                    _nextIndex -= _bufferFilledLength;
                    _bufferFilledLength = 0;
                }
            }
            else if (_bufferFilledLength > 0) // && _nextIndex == 0
            {
                // If we had previous content but couldn't consume any, the buffer is too small - expand it
                char[] expandedBuffer = new char[_buffer.Length * 2];
                Buffer.BlockCopy(_buffer, 0, expandedBuffer, 0, 2 * _bufferFilledLength);
                _buffer = expandedBuffer;
            }

            // Read more to fill the remaining buffer space
            int addedLength = _reader.ReadBlock(_buffer, _bufferFilledLength, _buffer.Length - _bufferFilledLength);
            _bufferFilledLength += addedLength;

            // Always return true if we may have shifted or read more; the caller needs to know to try to process it
            return true;
        }

        /// <summary>
        ///  Return the string values of the current row columns.
        /// </summary>
        public List<string> Current()
        {
            return _currentRow;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            _reader?.Dispose();
            _reader = null;
        }
    }
}
