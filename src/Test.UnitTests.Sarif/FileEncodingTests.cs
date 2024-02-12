// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using FluentAssertions;
using FluentAssertions.Execution;

using Microsoft.CodeAnalysis.Sarif.Driver;

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
            var assertionScope = new AssertionScope();

            var binaryExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".cer",
                ".der",
                ".pfx",
                ".dll",
                ".exe",
            };

            var observedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var fileSpecifier = new FileSpecifier("*", recurse: false);

            foreach (string fileName in fileSpecifier.Files)
            {
                string extension = Path.GetExtension(fileName);
                if (observedExtensions.Contains(extension)) { continue; }
                observedExtensions.Add(extension);

                using FileStream reader = File.OpenRead(fileName);
                int bufferSize = 1024;
                byte[] bytes = new byte[bufferSize];
                reader.Read(bytes, 0, bufferSize);
                bool isTextual = FileEncoding.IsTextualData(bytes);

                if (!isTextual)
                {
                    binaryExtensions.Should().Contain(extension, because: $"'{fileName}' was classified as a binary file");
                }
            }
        }

        private void ValidateIsBinary(string fileName)
        {
            if (!File.Exists(fileName))
            {
                fileName = Path.Combine(Environment.CurrentDirectory, fileName);
            }

            fileName = Path.Combine(Environment.CurrentDirectory, fileName);
            using FileStream reader = File.OpenRead(fileName);
            int bufferSize = 1024;
            byte[] bytes = new byte[bufferSize];
            reader.Read(bytes, 0, bufferSize);
            FileEncoding.IsTextualData(bytes).Should().BeFalse(because: $"{fileName} is a binary file");
        }

        [Fact]
        public void FileEncoding_UnicodeDataIsTextual()
        {
            using var assertionScope = new AssertionScope();

            var sb = new StringBuilder();
            string[] unicodeTexts = new[]
            {
                "американец",
                "Generates a warning and an error for each of :  foo  foo \r\n" // Challenging for the classifer; found by accident.
            };

            foreach (string unicodeText in unicodeTexts)
            {
                foreach (Encoding encoding in new[] { Encoding.Unicode, Encoding.UTF8, Encoding.BigEndianUnicode, Encoding.UTF32 })
                {
                    byte[] input = encoding.GetBytes(unicodeText);
                    FileEncoding.IsTextualData(input).Should().BeTrue(because: $"'{unicodeText}' encoded as '{encoding.EncodingName}' should not be classified as binary data");
                }
            }
        }

        [Fact]
        public void FileEncoding_BinaryDataIsBinary()
        {
            using var assertionScope = new AssertionScope();

            foreach (string binaryName in new[] { "Certificate.cer", "Certificate.der", "PasswordProtected.pfx" })
            {
                string resourceName = $"Test.UnitTests.Sarif.TestData.FileEncoding.{binaryName}";
                Stream resource = typeof(FileEncodingTests).Assembly.GetManifestResourceStream(resourceName);

                var artifact = new EnumeratedArtifact(new FileSystem())
                {
                    Uri = new Uri(resourceName, UriKind.Relative),
                    Stream = resource,
                };

                artifact.Bytes.Should().NotBeNull(because: $"'{binaryName}' should be classified as binary data.");
            }
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
            using var assertionScope = new AssertionScope();

            Stream resource = typeof(FileEncodingTests).Assembly.GetManifestResourceStream($"Test.UnitTests.Sarif.TestData.FileEncoding.{encodingName}.txt");
            using var reader = new StreamReader(resource, Encoding.UTF8);

            int current;
            while ((current = reader.Read()) != -1)
            {
                char ch = (char)current;
                byte[] input = encoding.GetBytes(new[] { ch });

                if (ch < 0x20) { continue; }

                string unicodeText = "\\u" + ((int)ch).ToString("d4");
                FileEncoding.IsTextualData(input).Should().BeTrue(because: $"{encodingName} character '{unicodeText}' ({encoding.GetString(input)}) should not classify as binary data.");
            }
        }
    }
}
