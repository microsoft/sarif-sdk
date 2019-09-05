// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AzureDevOpsCrawlers.Common.IO
{
    /// <summary>
    ///  CsvWriter provides a simple interface to write valid Csv files.
    ///  All strings are wrapped in quotes and quotes are doubled, which
    ///  is the simplest way to produce a CSV conforming to RFC 4180.
    ///  
    ///  Usage:
    ///  using(CsvWriter writer = new CsvWriter("Output.Csv"))
    ///  {
    ///     writer.SetColumns(new string[] { "Name", "Address" });
    ///     
    ///     foreach(...)
    ///     {
    ///         writer.Write(name);
    ///         writer.Write(address);
    ///         writer.NextRow();
    ///     }
    ///  }
    /// </summary>
    /// <remarks>
    ///  Based on: https://github.com/Microsoft/elfie-arriba/blob/master/Elfie/Elfie/Serialization/CsvWriter.cs
    /// </remarks>
    public class CsvWriter : IDisposable
    {
        private StreamWriter _writer;
        private bool _writeHeaderRow;
        private int _columnCount;

        private bool _inPartialColumn;
        private int _currentRowColumnCount;
        private int _rowCountWritten;

        private char[] _copyBuffer;

        public Encoding Encoding
        {
            get
            {
                return _writer.Encoding;
            }
        }

        public CsvWriter(string filePath, bool writeHeaderRow = true) : this(new FileStream(EnsureDirectoryCreated(filePath), FileMode.Create, FileAccess.Write, FileShare.Read), writeHeaderRow)
        { }

        public CsvWriter(Stream stream, bool writeHeaderRow = true)
        {
            _writer = new StreamWriter(stream, Encoding.UTF8);
            _writeHeaderRow = writeHeaderRow;
            _rowCountWritten = 0;

            _copyBuffer = new char[1024];
        }

        private static string EnsureDirectoryCreated(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!String.IsNullOrEmpty(directoryPath)) { Directory.CreateDirectory(directoryPath); }

            return filePath;
        }

        public int RowCountWritten => _rowCountWritten;

        public bool RemoveNewlines { get; set; }

        public void SetColumns(IEnumerable<string> columnNames)
        {
            _columnCount = columnNames.Count();

            if (_writeHeaderRow)
            {
                foreach (string columnName in columnNames)
                {
                    Write(columnName);
                }

                NextRow();
            }
        }

        public void NextRow()
        {
            WriteRowDelimiter();
            _currentRowColumnCount = 0;
            _rowCountWritten++;
        }

        public void Write(int value)
        {
            Write(value.ToString());
        }

        public void Write(string value)
        {
            if (_currentRowColumnCount >= _columnCount) { throw new InvalidOperationException(String.Format("Writing too many columns for row {0:n0}. Wrote {1:n0}, expected {2:n0} columns.", _rowCountWritten, _currentRowColumnCount, _columnCount)); }
            if (_inPartialColumn) { throw new InvalidOperationException("Write was called while in a multi-part column. Call WriteValueStart, WriteValuePart, and WriteValueEnd only for partial columns."); }
            if (_currentRowColumnCount > 0) { WriteCellDelimiter(); }
            WriteEscaped(value);
            _currentRowColumnCount++;
        }

        public void WriteValueStart()
        {
            if (_currentRowColumnCount >= _columnCount) { throw new InvalidOperationException(String.Format("Writing too many columns for row {0:n0}. Wrote {1:n0}, expected {2:n0} columns.", _rowCountWritten, _currentRowColumnCount, _columnCount)); }
            if (_currentRowColumnCount > 0) { WriteCellDelimiter(); }
            _inPartialColumn = true;
        }

        public void WriteValuePart(string value)
        {
            if (!_inPartialColumn) { throw new InvalidOperationException("WriteValueStart must be called before WriteValuePart."); }
            WriteEscaped(value);
        }

        public void WriteValueEnd()
        {
            if (!_inPartialColumn) { throw new InvalidOperationException("WriteValueEnd called but WriteValueStart was never called."); }
            _inPartialColumn = false;
        }

        private void WriteCellDelimiter()
        {
            _writer.Write(',');
        }

        private void WriteRowDelimiter()
        {
            _writer.Write("\r\n");
        }

        private void WriteEscaped(string value)
        {
            if (String.IsNullOrEmpty(value)) { return; }
            bool removeNewlines = this.RemoveNewlines;
            int nextWriteIndex = 0;

            _writer.Write('"');

            for (int i = 0; i < value.Length; ++i)
            {
                char c = value[i];
                if (c == '"')
                {
                    WriteStringPart(value, nextWriteIndex, (i + 1) - nextWriteIndex);
                    nextWriteIndex = i;
                }
                else if (removeNewlines && (c == '\r' || c == '\n'))
                {
                    WriteStringPart(value, nextWriteIndex, i - nextWriteIndex);
                    nextWriteIndex = i + 1;
                }
            }

            WriteStringPart(value, nextWriteIndex);
            _writer.Write('"');
        }

        private void WriteStringPart(string value, int index, int length = -1)
        {
            if (length == -1) { length = value.Length - index; }

            // Ignore empty writes
            if (length == 0) { return; }

            // Ensure there's enough room to copy
            if (length > _copyBuffer.Length) { _copyBuffer = new char[Math.Max(length, _copyBuffer.Length * 2)]; }

            // Copy the string to a char[] (so it can be partially written without substring allocations
            value.CopyTo(index, _copyBuffer, 0, length);

            // Write the string part
            _writer.Write(_copyBuffer, 0, length);
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                if (_currentRowColumnCount > 0) { NextRow(); }
                _writer.Dispose();
                _writer = null;
            }
        }
    }
}