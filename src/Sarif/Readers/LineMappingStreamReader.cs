// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

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
        private long _firstLineCharsBeforeBuffer;
        private int _lastLineChars;

        // Keep a copy of the last read buffer and the valid range of it so we can count bytes within the requested line.
        private char[] _buffer;
        private int _bufferIndex;
        private int _bufferLength;

        // Track the last character index to byte index requested
        private int _lastMappedLineStartIndex;
        private long _lastMappedCharInLine;
        private int _lastMappedByteCountReturned;

        // Json.NET can overflow LinePosition for huge minified JSON.
        // Track and attempt to correct for it.
        private readonly OverflowCorrector _overflowCorrector;

        public LineMappingStreamReader(Stream stream) : base(FindBomWidth(stream, out int bomWidth))
        {
            // Record the width of any BOM, so that byte offsets we return are correct (base StreamReader will read and hide BOM)
            _bytesReadPreviously = bomWidth;

            // (1, 1) is the 0th byte, so the line number before the read is 1 and there is one character (the newline before the first line) before the first read.
            _firstLineNumber = 1;
            _firstLineCharsBeforeBuffer = 1;

            _overflowCorrector = new OverflowCorrector();
        }

        private static Stream FindBomWidth(Stream stream, out int bomWidth)
        {
            long previousPosition = stream.Position;

            // Read four bytes
            byte[] buffer = new byte[4];
            stream.Seek(0, SeekOrigin.Begin);
            int countRead = stream.Read(buffer, 0, 4);

            // Check for BOMs and record byte count
            if (buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                bomWidth = 2;
            }
            else if(buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                bomWidth = 2;
            }
            else if(buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                bomWidth = 3;
            }
            else if(buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xFE && buffer[3] == 0xFF)
            {
                bomWidth = 4;
            }
            else
            {
                bomWidth = 0;
            }

            // Restore the stream to the prior position
            stream.Seek(previousPosition, SeekOrigin.Begin);

            return stream;
        }

        public long LineAndCharToOffset(int line, long charInLine)
        {
            if (line == 0 && charInLine == 0) { return 0; }
            if (line < _firstLineNumber || line > _firstLineNumber + _lineCount) { throw new ArgumentOutOfRangeException($"Line must be in the range of lines last read, ({_firstLineNumber} - {_firstLineNumber + _lineCount}). It was {line}."); }

            // Track and correct for overflows from JSON.net
            charInLine = _overflowCorrector.CorrectForOverflow(line, charInLine);

            long bytesInLine;

            if (line == _firstLineNumber)
            {
                if (charInLine == _firstLineCharsBeforeBuffer - 1) { return _bytesReadPreviously - 1; }
                if (charInLine < _firstLineCharsBeforeBuffer || charInLine - _firstLineCharsBeforeBuffer > _bufferLength)
                {
                    throw new ArgumentOutOfRangeException($"Line {line} chars ({_firstLineCharsBeforeBuffer} to {_firstLineCharsBeforeBuffer + _bufferLength} in range. {charInLine} requested was out of range.");
                }

                // For the first line, the offset is total bytes read plus byte count for the characters in the line which are in the current buffer.
                long charsInBufferForLine = charInLine - _firstLineCharsBeforeBuffer;
                bytesInLine = BytesInLine(_bufferIndex, charsInBufferForLine);
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
            bytesInLine = BytesInLine(_bufferIndex + newlineCharIndex, charInLine);

            // Return the byte offset to this specific character
            long position = newlineByteOffset + bytesInLine;
            return position;
        }

        private long BytesInLine(int lineStartCharIndex, long charInLine)
        {
            if (_bufferLength == _bytesRead)
            {
                // If the buffer is all single byte characters, there's no mapping to do
                return charInLine;
            }

            int byteCount;

            if (_lastMappedLineStartIndex == lineStartCharIndex && _lastMappedCharInLine <= charInLine)
            {
                // If we mapped this line, just count bytes after the last character
                byteCount = _lastMappedByteCountReturned + this.CurrentEncoding.GetByteCount(_buffer, (int)_lastMappedCharInLine, (int)(charInLine - _lastMappedCharInLine));
            }
            else
            {
                // Otherwise, we must count bytes for the characters found
                byteCount = this.CurrentEncoding.GetByteCount(_buffer, lineStartCharIndex, (int)charInLine);
            }

            _lastMappedLineStartIndex = lineStartCharIndex;
            _lastMappedCharInLine = charInLine;
            _lastMappedByteCountReturned = byteCount;
            return byteCount;
        }

        public char? CharAt(int line, long charInLine)
        {
            charInLine = _overflowCorrector.CorrectForOverflow(line, charInLine);

            // Map the Line and Char to an index in the buffer
            int charIndex;
            if (line == _firstLineNumber)
            {
                charIndex = (int)(charInLine - _firstLineCharsBeforeBuffer);
            }
            else
            {
                int lineIndex = line - ((int)_firstLineNumber + 1);

                // If the line is not in the buffer, return false
                if (lineIndex < 0 || lineIndex > _lineCount)
                {
                    return null;
                }

                charIndex = (int)(_lineStartIndices[lineIndex] + charInLine);
            }

            // If the desired character isn't in the buffer, return false
            if (charIndex < 0 || charIndex >= _bufferLength)
            {
                return null;
            }

            // Otherwise, return the desired character
            return _buffer[_bufferIndex + charIndex];
        }

        public override int Read(char[] buffer, int index, int count)
        {
            // Clear previously cached results
            _lastMappedLineStartIndex = -1;

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
            if (_buffer == null || _buffer.Length < buffer.Length) { _buffer = new char[buffer.Length]; }
            Buffer.BlockCopy(buffer, 2 * index, _buffer, 0, 2 * charsRead);
            _bufferIndex = 0;
            _bufferLength = charsRead;

            // Ensure space to hold start of each line
            if (_lineStartByteOffsets == null || _lineStartByteOffsets.Length < charsRead) { _lineStartByteOffsets = new int[charsRead]; }
            if (_lineStartIndices == null || _lineStartIndices.Length < charsRead) { _lineStartIndices = new int[charsRead]; }
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
