// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Converters.TextFormats
{
    /// <summary>
    ///  TsvReader provides a simple interface for reading TSV files.
    /// </summary>
    /// <remarks>
    ///  Loosely based on: https://github.com/Microsoft/elfie-arriba/blob/master/Elfie/Elfie/Serialization/TsvReader.cs
    /// </remarks>
    internal class TsvReader : BaseTabularReader
    {
        public TsvReader(string filePath, int bufferSize = DefaultBufferSize) : this(File.OpenRead(filePath), bufferSize)
        { }

        public TsvReader(Stream stream, int bufferSize = DefaultBufferSize) : base(stream, bufferSize)
        { }

        public override bool NextRow()
        {
            bool fileHasMore = true;

            // Read more content if needed, returning false without incrementing RowCountRead if done
            if (_nextIndex >= _bufferFilledLength)
            {
                fileHasMore = RefillBuffer();
                if (!fileHasMore) { return false; }
            }

            _currentRow.Clear();
            RowCountRead++;

            while (true)
            {
                // Find the end of the current cell
                int cellEnd = NextCellEnd();

                // If we didn't find a delimiter and there might be more content, read more and try again
                if (cellEnd >= _bufferFilledLength && fileHasMore)
                {
                    fileHasMore = RefillBuffer();
                    continue;
                }

                // If we found a delimiter or end of file, read in the cell value
                _currentRow.Add(new string(_buffer, _nextIndex, cellEnd - _nextIndex));
                _nextIndex = cellEnd + 1;

                // If the last delimiter was not a cell delimiter, we finished reading the row
                if (!(cellEnd < _bufferFilledLength && _buffer[cellEnd] == '\t'))
                {
                    break;
                }
            }

            // Skip newline for \r\n to set _nextIndex to the beginning of the next line
            if (_nextIndex <= _bufferFilledLength && _buffer[_nextIndex - 1] == '\r') { _nextIndex++; }

            return true;
        }

        private int NextCellEnd()
        {
            int cellEnd = _nextIndex;
            for (; cellEnd < _bufferFilledLength; ++cellEnd)
            {
                if (_buffer[cellEnd] == '\t' || _buffer[cellEnd] == '\r' || _buffer[cellEnd] == '\n')
                {
                    break;
                }
            }

            return cellEnd;
        }
    }
}
