// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
        private struct FilePosition
        {
            public int LineNumber { get; set; }
            public int CharInLine { get; set; }
            public int BufferIndex { get; set; }
            public long ByteOffset { get; set; }

            public FilePosition(int lineNumber, int charInLine, int bufferIndex, long byteOffset)
            {
                this.LineNumber = lineNumber;
                this.CharInLine = charInLine;
                this.BufferIndex = bufferIndex;
                this.ByteOffset = byteOffset;
            }
        }

        // Track the position corresponding to the first buffer byte and the last one returned
        private FilePosition _bufferStartPosition;
        private FilePosition _lastReturnedPosition;

        // Keep a copy of the last read buffer and the valid range of it so we can count bytes within the requested line.
        private char[] _buffer;
        private int _bufferLength;

        public LineMappingStreamReader(Stream stream) : base(stream)
        {
            _bufferStartPosition = new FilePosition(1, 1, 0, 0);
            _lastReturnedPosition = _bufferStartPosition;
        }

        public long LineAndCharToOffset(int line, int charInLine)
        {
            if (line == 0 && charInLine == 0) return 0;

            FilePosition position = CountUpTo(line, charInLine);
            _lastReturnedPosition = position;

            return position.ByteOffset;
        }

        private FilePosition CountUpTo(int line, int charInLine)
        {
            if (line < _bufferStartPosition.LineNumber) throw new ArgumentOutOfRangeException($"Line must be in the range of lines last read, from ({_bufferStartPosition.LineNumber}). Request was for {line}.");

            FilePosition start = _bufferStartPosition;

            // Start from the last returned position if possible
            if (_lastReturnedPosition.LineNumber < line || (_lastReturnedPosition.LineNumber == line && _lastReturnedPosition.CharInLine <= charInLine))
            {
                start = _lastReturnedPosition;
            }

            // Calculate where we're scanning the buffer from
            FilePosition current = start;
            int bufferIndex = start.BufferIndex;

            // Find the correct newline
            for (; current.LineNumber < line && bufferIndex < _bufferLength; bufferIndex++)
            {
                if (_buffer[bufferIndex] == '\n')
                {
                    current.ByteOffset = current.ByteOffset + this.CurrentEncoding.GetByteCount(_buffer, current.BufferIndex, bufferIndex - current.BufferIndex);

                    current.LineNumber++;
                    current.CharInLine = 0;
                    current.BufferIndex = bufferIndex;
                }
            }

            // Count bytes on this line
            if (current.CharInLine < charInLine)
            {
                int charsToAdd = charInLine - current.CharInLine;
                if (current.BufferIndex + charsToAdd > _bufferLength) throw new ArgumentOutOfRangeException($"Position must be in buffer from last read. ({line}, {charInLine}) requested; ({current.LineNumber}, {current.CharInLine + _bufferLength - current.BufferIndex}) is end of buffer.");

                current.ByteOffset = current.ByteOffset + this.CurrentEncoding.GetByteCount(_buffer, current.BufferIndex, charsToAdd);

                current.CharInLine = charInLine;
                current.BufferIndex += charsToAdd;
            }

            // Return the found position
            return current;
        }

        private FilePosition CountUpTo(int index)
        {
            if (index < 0 || index >= _bufferLength) throw new IndexOutOfRangeException("index");

            FilePosition current = (_lastReturnedPosition.BufferIndex <= index ? _lastReturnedPosition : _bufferStartPosition);

            // Find the line number of the index
            int lastNewlineIndex = current.BufferIndex - current.CharInLine;

            int bufferIndex = current.BufferIndex;
            for (; bufferIndex <= index; bufferIndex++)
            {
                if (_buffer[bufferIndex] == '\n')
                {
                    current.LineNumber++;
                    lastNewlineIndex = bufferIndex;
                }
            }

            // Calculate the char of the index
            current.CharInLine = index - lastNewlineIndex;

            // Count bytes up to the index
            current.ByteOffset += this.CurrentEncoding.GetByteCount(_buffer, current.BufferIndex, index - current.BufferIndex);

            return current;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            // Count the rest of the last buffer and store the position where this buffer starts
            if (_bufferLength > 0)
            {
                FilePosition endOfBuffer = CountUpTo(_bufferLength - 1);
                endOfBuffer.CharInLine++;
                endOfBuffer.ByteOffset++;
                endOfBuffer.BufferIndex = 0;

                _bufferStartPosition = endOfBuffer;
                _lastReturnedPosition = endOfBuffer;
            }

            // Read the new buffer
            int charsRead = base.Read(buffer, index, count);

            // Copy buffer so we can map char in line to byte count (real buffer can be shifted by reader, invalidating indices)
            if (_buffer == null || _buffer.Length < buffer.Length) _buffer = new char[buffer.Length];
            Buffer.BlockCopy(buffer, 2 * index, _buffer, 0, 2 * charsRead);
            _bufferLength = charsRead;

            return charsRead;
        }
    }
}
