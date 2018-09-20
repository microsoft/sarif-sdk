// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    /// <summary>
    ///  LineMappingStreamReader is a StreamReader which additionally tracks the byte offsets of each line start
    ///  in the part of the file currently read. This allows it to map a line and char back to a byte offset
    ///  to allow consumers to seek back to the position later.
    /// </summary>
    internal class LineMappingStreamReader : StreamReader
    {
        // Track bytes read before the current read (to return absolute line offsets) and in the current read
        private long _bytesReadPreviously;
        private int _bytesRead;

        // Keep the char index and byte offset of the start of each line in the currently read buffer (byteOffset + _bytesReadPreviously => LineOffset)
        private int[] _lineStartIndices;
        private int[] _lineStartByteOffsets;
        private long _lineCount;

        // Track the first line number and byte offset because it can start before the buffer range
        private long _firstLineNumber;
        private int _firstLineCharsBeforeBuffer;
        private int _lastLineChars;

        // Keep a copy of the last read buffer and the valid range of it so we can count bytes within the requested line.
        private char[] _buffer;
        private int _bufferIndex;
        private int _bufferLength;


        public LineMappingStreamReader(Stream stream) : base(stream)
        {
            // (1, 1) is the 0th byte, so the line number before the read is 1 and there is one character (the newline before the first line) before the first read.
            _firstLineNumber = 1;
            _firstLineCharsBeforeBuffer = 1;
        }

        public long LineAndCharToOffset(int line, int charInLine)
        {
            if (line == 0 && charInLine == 0) return 0;
            if (line < _firstLineNumber || line > _firstLineNumber + _lineCount) throw new ArgumentOutOfRangeException($"Line must be in the range of lines last read, ({_firstLineNumber} - {_firstLineNumber + _lineCount}). It was {line}.");

            int bytesInLine;

            if (line == _firstLineNumber)
            {
                if (charInLine < _firstLineCharsBeforeBuffer || charInLine - _firstLineCharsBeforeBuffer > _bufferLength) throw new ArgumentOutOfRangeException($"Line {line} chars ({_firstLineCharsBeforeBuffer} to {_firstLineCharsBeforeBuffer + _bufferLength} in range. {charInLine} requested was out of range.");

                // For the first line, the offset is total bytes read plus byte count for the characters in the line which are in the current buffer.
                int charsInBufferForLine = charInLine - _firstLineCharsBeforeBuffer;
                bytesInLine = this.CurrentEncoding.GetByteCount(_buffer, _bufferIndex, charsInBufferForLine);
                return _bytesReadPreviously + bytesInLine;
            }

            // Find the newline starting the line
            long newlineByteOffset = _bytesReadPreviously + _lineStartByteOffsets[line - (_firstLineNumber + 1)];
            int newlineCharIndex = _lineStartIndices[line - (_firstLineNumber + 1)];

            // Get the byte count for the characters in the line
            if (newlineCharIndex + charInLine > _bufferLength)
            {
                throw new ArgumentOutOfRangeException($"Line {line} up to char {charInLine} isn't available in buffer. Only {_bufferLength - newlineCharIndex} characters available.");
            }
            bytesInLine = this.CurrentEncoding.GetByteCount(_buffer, _bufferIndex + newlineCharIndex, charInLine);

            // Return the byte offset to this specific character
            long position = newlineByteOffset + bytesInLine;
            return position;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            // Track the first line number and char total before the current buffer
            if (_lineCount > 0)
            {
                _firstLineNumber += _lineCount;
                _firstLineCharsBeforeBuffer = 0;
            }

            _firstLineCharsBeforeBuffer += _lastLineChars;

            // Maintain total byte count read
            _bytesReadPreviously += _bytesRead;

            // Read the new buffer
            int charsRead = base.Read(buffer, index, count);

            // Copy buffer so we can map char in line to byte count (real buffer can be shifted by reader, invalidating indices)
            if (_buffer == null || _buffer.Length < buffer.Length) _buffer = new char[buffer.Length];
            Buffer.BlockCopy(buffer, 2 * index, _buffer, 0, 2 * charsRead);
            _bufferIndex = 0;
            _bufferLength = charsRead;

            // Ensure space to hold start of each line
            if (_lineStartByteOffsets == null || _lineStartByteOffsets.Length < charsRead) _lineStartByteOffsets = new int[charsRead];
            if (_lineStartIndices == null || _lineStartIndices.Length < charsRead) _lineStartIndices = new int[charsRead];
            _lineCount = 0;

            // Find each newline byte offset relative to current read
            int bytesRead = 0;
            int lastEnd = index;

            for (int i = index; i < index + charsRead; ++i)
            {
                if (buffer[i] == '\n')
                {
                    // Count bytes up to this line
                    bytesRead += this.CurrentEncoding.GetByteCount(buffer, lastEnd, i - lastEnd);
                    lastEnd = i;

                    // Store the char index and byte offset of the newline for this line
                    _lineStartIndices[_lineCount] = i - index;
                    _lineStartByteOffsets[_lineCount] = bytesRead;
                    _lineCount++;
                }
            }

            // Count bytes in last line until end of buffer
            if (lastEnd < index + charsRead)
            {
                bytesRead += this.CurrentEncoding.GetByteCount(buffer, lastEnd, (index + charsRead) - lastEnd);
            }

            // Count chars in the last line in this buffer
            _lastLineChars = (index + charsRead) - lastEnd;

            _bytesRead = bytesRead;
            return charsRead;
        }
    }
}
