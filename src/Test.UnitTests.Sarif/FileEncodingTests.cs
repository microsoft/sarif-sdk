// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{

    public class FileEncodingTests
    {
        [Fact]
        public void FileEncoding_NullBytesRaisesException()
        {
            Assert.Throws<ArgumentNullException>(() => FileEncoding.IsTextualData(null, 1, 1));
        }

        [Fact]
        public void FileEncoding_StartExceedsBufferLength()
        {
            // Start argument exceeds buffer size.
            Assert.Throws<ArgumentOutOfRangeException>(() => FileEncoding.IsTextualData(new byte[1], 1, 1));
        }

        [Fact]
        public void FileEncoding_FileEncoding_IsBinary()
        {
            ValidateIsBinary("Sarif.dll");
            ValidateIsBinary("Sarif.pdb");
        }

        private void ValidateIsBinary(string fileName)
        {
            fileName = Path.Combine(Environment.CurrentDirectory, fileName);
            using FileStream reader = File.OpenRead(fileName);
            int bufferSize = 1024;
            byte[] bytes = new byte[bufferSize];
            reader.Read(bytes, 0, bufferSize);
            FileEncoding.IsTextualData(bytes).Should().BeFalse();
        }

        [Fact]
        public void FileEncoding_UnicodeDataIsTextual()
        {
            var sb = new StringBuilder();
            string unicodeText = "американец";

            foreach (Encoding encoding in new[] { Encoding.Unicode, Encoding.UTF8, Encoding.BigEndianUnicode, Encoding.UTF32 })
            {
                byte[] input = encoding.GetBytes(unicodeText);
                if (!FileEncoding.IsTextualData(input))
                {
                    sb.AppendLine($"\tThe '{encoding.EncodingName}' encoding classified unicode text '{unicodeText}' as binary data.");
                }
            }

            sb.Length.Should().Be(0, because: $"all unicode strings should be classified as textual:{Environment.NewLine}{sb}");
        }

        [Fact]
        public void FileEncoding_BinaryDataIsBinary()
        {
            var sb = new StringBuilder();

            foreach (string binaryName in new[] { "Certificate.cer", "Certificate.der", "PasswordProtected.pfx" })
            {
                string resourceName = $"Test.UnitTests.Sarif.TestData.FileEncoding.{binaryName}";
                Stream resource = typeof(FileEncodingTests).Assembly.GetManifestResourceStream(resourceName);

                var artifact = new EnumeratedArtifact(new FileSystem())
                {
                    Uri = new Uri(resourceName, UriKind.Relative),
                    Stream = resource,
                };

                if (artifact.Bytes == null)
                {
                    sb.AppendLine($"\tBinary file '{binaryName}' was classified as textual data.");
                }
            }

            sb.Length.Should().Be(0, because: $"all binary files should be classified as binary:{Environment.NewLine}{sb}");
        }

        [Fact]
        public void FileEncoding_AllWindows1252EncodingsAreTextual()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ValidateEncoding("Windows-1252", Encoding.GetEncoding(1252));
        }

        [Fact]
        public void FileEncoding_AllUtf8EncodingsAreTextual()
        {
            ValidateEncoding("Utf8", Encoding.UTF8);
        }

        private static void ValidateEncoding(string encodingName, Encoding encoding)
        {
            var sb = new StringBuilder(65536 * 100);
            Stream resource = typeof(FileEncodingTests).Assembly.GetManifestResourceStream($"Test.UnitTests.Sarif.TestData.FileEncoding.{encodingName}.txt");
            using var reader = new StreamReader(resource, Encoding.UTF8);

            int current;
            while ((current = reader.Read()) != -1)
            {
                char ch = (char)current;
                byte[] input = encoding.GetBytes(new[] { ch });
                if (!FileEncoding.IsTextualData(input))
                {
                    string unicodeText = "\\u" + ((int)ch).ToString("d4");
                    sb.AppendLine($"\t{encodingName} character '{unicodeText}' ({encoding.GetString(input)}) was classified as binary data.");
                }
            }

            sb.Length.Should().Be(0, because: $"all {encodingName} encodable character should be classified as textual:{Environment.NewLine}{sb}");
        }
    }
}
