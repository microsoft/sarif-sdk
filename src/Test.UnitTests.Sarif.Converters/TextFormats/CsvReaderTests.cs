// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Converters.TextFormats;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class CsvReaderTests
    {
        [Fact]
        public void CsvReader_Basics()
        {
            int bufferSize = 64;

            // Empty file - verify no rows
            using (CsvReader reader = new CsvReader(StreamFromString(""), bufferSize))
            {
                Assert.False(reader.NextRow());

                // Verify double-dispose causes no problem
                reader.Dispose();
            }

            // Single row file, no trailing newline
            using (CsvReader reader = new CsvReader(StreamFromString("One,Two,Three"), bufferSize))
            {
                Assert.Equal(0, reader.RowCountRead);
                Assert.True(reader.NextRow());
                Assert.Equal(1, reader.RowCountRead);
                Assert.Equal("One|Two|Three", string.Join("|", reader.Current()));
                Assert.False(reader.NextRow());
                Assert.Equal(1, reader.RowCountRead);
            }

            // Empty values
            using (CsvReader reader = new CsvReader(StreamFromString(",Value,,"), bufferSize))
            {
                Assert.True(reader.NextRow());
                Assert.Equal("|Value||", string.Join("|", reader.Current()));
                Assert.False(reader.NextRow());
            }

            // Newline variation and trailing newline
            using (CsvReader reader = new CsvReader(StreamFromString("One\nTwo\r\nThree\r\n"), bufferSize))
            {
                Assert.True(reader.NextRow());
                Assert.Equal("One", string.Join("|", reader.Current()));
                Assert.True(reader.NextRow());
                Assert.Equal("Two", string.Join("|", reader.Current()));
                Assert.True(reader.NextRow());
                Assert.Equal("Three", string.Join("|", reader.Current()));
                Assert.False(reader.NextRow());
            }

            // Row requiring a buffer resize and verify nothing is missed
            string oneHundredColumns = string.Join(",", Enumerable.Range(100, 100).Select(i => i.ToString()));
            using (CsvReader reader = new CsvReader(StreamFromString(oneHundredColumns), bufferSize))
            {
                Assert.True(reader.NextRow());
                Assert.Equal(100, reader.Current().Count);
                Assert.False(reader.NextRow());
            }

            // Value exactly 2x buffer, requiring two buffer resizes to be read
            string valueRequiringBufferExpand = new string('0', 128);
            using (CsvReader reader = new CsvReader(StreamFromString($"One,Two,Three\r\nSecond,Row\r\n{valueRequiringBufferExpand}"), bufferSize))
            {
                Assert.True(reader.NextRow());
                Assert.Equal("One|Two|Three", string.Join("|", reader.Current()));
                Assert.True(reader.NextRow());
                Assert.Equal("Second|Row", string.Join("|", reader.Current()));
                Assert.True(reader.NextRow());
                Assert.Single(reader.Current());
                Assert.Equal(valueRequiringBufferExpand, reader.Current()[0]);
            }

            // '\r' exactly at buffer boundary, requiring refill to track the unread '\n' to ignore
            using (CsvReader reader = new CsvReader(StreamFromString($"{new string('0', 63)}\r\nNextRow\r\n"), bufferSize))
            {
                Assert.True(reader.NextRow());
                Assert.Equal(new string('0', 63), string.Join("|", reader.Current()));
                Assert.True(reader.NextRow());
                Assert.Equal("NextRow", string.Join("|", reader.Current()));
                Assert.False(reader.NextRow());
            }

            // Quoted value variations - quoted empty, quotes at start/end, quote in middle, unquoted after quoted, adjacent escaped quotes
            using (CsvReader reader = new CsvReader(StreamFromString("\"\",\"\"\"Around\"\"\",\"With\"\"in\",None,\"Many\"\"\"\"\"\nNextRow"), bufferSize))
            {
                Assert.True(reader.NextRow());
                Assert.Equal("|\"Around\"|With\"in|None|Many\"\"", string.Join("|", reader.Current()));
                Assert.True(reader.NextRow());
                Assert.Equal("NextRow", string.Join("|", reader.Current()));
                Assert.False(reader.NextRow());
            }

            // Exception for unescaped quote not at cell end
            using (CsvReader reader = new CsvReader(StreamFromString("\"Unescaped\"Quote,"), bufferSize))
            {
                Assert.Throws<IOException>(() => reader.NextRow());
            }
        }

        [Fact]
        public void CsvReader_File()
        {
            string filePath = "CsvReader.Sample.csv";
            File.WriteAllText(filePath, "One,Two,Three\r\nFour,Five,Six");
            Assert.Equal(2, ParseFile(filePath));
        }

        // Uncomment for performance testing or fuzzing
        //[Fact]
        //public void CsvReader_Performance()
        //{
        //    string largeFilePath = "<provide>";
        //    int rowCountRead = ParseFile(largeFilePath);
        //}

        //[Fact]
        //public void CsvReader_Spam()
        //{
        //    string folderPath = @"C:\Download";
        //    foreach (string filePath in Directory.EnumerateFiles(folderPath, "*.csv", SearchOption.AllDirectories))
        //    {
        //        int rowCountRead = ParseFile(filePath);
        //    }
        //}

        private int ParseFile(string filePath)
        {
            using (CsvReader reader = new CsvReader(filePath))
            {
                while (reader.NextRow())
                { }

                return reader.RowCountRead;
            }
        }

        private MemoryStream StreamFromString(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }
    }
}
