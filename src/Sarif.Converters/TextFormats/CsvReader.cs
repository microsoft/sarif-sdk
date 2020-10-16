// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Converters.TextFormats
{
    /// <summary>
    ///  CsvReader provides a simple interface for reading CSV files.
    ///  
    ///  Usage:
    ///  using(CsvReader reader = new CsvReader("Input.csv"))
    ///  {
    ///      while(reader.NextRow())
    ///      {
    ///           List&lt;string&rt; row = reader.Current();
    ///           ...
    ///      }
    ///  }
    /// </summary>
    /// <remarks>
    ///  Loosely based on: https://github.com/Microsoft/elfie-arriba/blob/master/Elfie/Elfie/Serialization/CsvReader.cs
    /// </remarks>
    internal class CsvReader : BaseTabularReader
    {
        public CsvReader(string filePath, int bufferSize = DefaultBufferSize) : this(File.OpenRead(filePath), bufferSize)
        { }

        public CsvReader(Stream stream, int bufferSize = DefaultBufferSize) : base(stream, bufferSize)
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
                int cellEnd = _nextIndex;
                bool quoted = (cellEnd < _bufferFilledLength && _buffer[cellEnd] == '"');

                // Find the end of the current cell
                cellEnd = NextCellEnd(quoted, out bool escapedQuotes);

                // If we didn't find a delimiter and there might be more content, read more and try again
                if (cellEnd >= _bufferFilledLength && fileHasMore)
                {
                    fileHasMore = RefillBuffer();
                    continue;
                }

                // If we found a delimiter or end of file, read in the cell value
                string value;
                if (quoted)
                {
                    value = new string(_buffer, _nextIndex + 1, cellEnd - _nextIndex - 2);
                    if (escapedQuotes) { value = value.Replace("\"\"", "\""); }
                }
                else
                {
                    value = new string(_buffer, _nextIndex, cellEnd - _nextIndex);
                }
                _currentRow.Add(value);
                _nextIndex = cellEnd + 1;

                // If the last delimiter was not a cell delimiter, we finished reading the row
                if (!(cellEnd < _bufferFilledLength && _buffer[cellEnd] == ','))
                {
                    break;
                }
            }

            // Skip newline for \r\n to set _nextIndex to the beginning of the next line
            if (_nextIndex <= _bufferFilledLength && _buffer[_nextIndex - 1] == '\r') { _nextIndex++; }

            return true;
        }

        /// <summary>
        ///  Return the index of the comma or newline delimiter terminating the cell value
        ///  starting at _nextIndex, or return _bufferFilledLength if one wasn't found
        ///  in the buffer range.
        /// </summary>
        private int NextCellEnd(bool quoted, out bool escapedQuotes)
        {
            escapedQuotes = false;
            int i = _nextIndex;

            if (!quoted)
            {
                // Unquoted - look for cell or row delimiter
                for (; i < _bufferFilledLength; ++i)
                {
                    if (_buffer[i] == ',' || _buffer[i] == '\r' || _buffer[i] == '\n')
                    {
                        break;
                    }
                }
            }
            else
            {
                // Quoted - look *after opening quote* for unescaped quote followed by delimiter (or end)
                i++;
                for (; i < _bufferFilledLength; ++i)
                {
                    if (_buffer[i] == '"')
                    {
                        i++;

                        if (i == _bufferFilledLength)
                        {
                            // Out of content
                            break;
                        }
                        else if (_buffer[i] == '"')
                        {
                            // Escaped quote. i++ skips the second one, so next iteration we'll look after both of these quotes
                            escapedQuotes = true;
                            continue;
                        }
                        else if (_buffer[i] == ',' || _buffer[i] == '\r' || _buffer[i] == '\n')
                        {
                            // Definitive delimiter
                            break;
                        }
                        else
                        {
                            // Quote without another quote (escaped quote) or a delimiter afterward - invalid CSV
                            throw new IOException($"CSV found with unescaped quote which didn't terminate cell.");
                        }
                    }
                }
            }

            // Return index of delimiter, or _bufferFilledLength if we couldn't find one
            return i;
        }
    }
}