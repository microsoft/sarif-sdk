// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Test.UnitTests.Sarif
{
    public class ZipArchiveArtifactTests
    {
        [Fact]
        public void ZipArchiveArtifact_BytesAndContentsCannotBeSet()
        {
            var doesNotExist = new Uri("file://does-not-exist.zip");
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextualArchiveData();
            var zipArchiveArtifact = new ZipArchiveArtifact(doesNotExist, testData.archive, testData.entry);

            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Contents = string.Empty);
            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Bytes = []);
        }

        [Fact]
        public void ZipArchiveArtifact_EncodingNotImplemented()
        {
            var doesNotExist = new Uri("file://does-not-exist.zip");
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextualArchiveData();
            var zipArchiveArtifact = new ZipArchiveArtifact(doesNotExist, testData.archive, testData.entry);

            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Encoding);
            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Encoding = Encoding.UTF8);
        }

        [Fact]
        public void ZipArchiveArtifact_SettingStreamNotImplemented()
        {
            var doesNotExist = new Uri("file://does-not-exist.zip");
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextualArchiveData();
            var zipArchiveArtifact = new ZipArchiveArtifact(doesNotExist, testData.archive, testData.entry);

            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Stream = null);
        }

        [Fact]
        public void ZipArchiveArtifact_SettingSizeInBytesNotImplemented()
        {
            var doesNotExist = new Uri("file://does-not-exist.zip");
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextualArchiveData();
            var zipArchiveArtifact = new ZipArchiveArtifact(doesNotExist, testData.archive, testData.entry);

            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.SizeInBytes = null);
        }

        [Fact]
        public void ZipArchiveArtifact_NonNullArchiveAndEntryAreRequired()
        {
            var doesNotExist = new Uri("file://does-not-exist.zip");
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextualArchiveData();

            Assert.Throws<ArgumentNullException>(() => new ZipArchiveArtifact(archiveUri: doesNotExist, archive: null, testData.entry));
            Assert.Throws<ArgumentNullException>(() => new ZipArchiveArtifact(archiveUri: null, testData.archive, entry: null));
        }

        [Fact]
        public void ZipArchiveArtifact_MixedTextAndBinaryZipEntry()
        {
            string textData = Guid.NewGuid().ToString();

            string filePath = this.GetType().Assembly.Location;
            using FileStream reader = File.OpenRead(filePath);
            byte[] binaryData = new byte[1024];
            int read = reader.Read(binaryData, 0, binaryData.Length);

            var binaryExtensions = new HashSet<string> { ".dll", ".exe" };
            ZipArchive archive = CreateMixedZipArchive(textData, binaryData);

            var doesNotExist = new Uri("file://does-not-exist.zip");
            var zipArchiveArtifact = new ZipArchiveArtifact(doesNotExist, archive, archive.Entries.First(), binaryExtensions);
            zipArchiveArtifact.IsBinary.Should().BeFalse();
            zipArchiveArtifact.Contents.Should().NotBeNull();
            zipArchiveArtifact.Contents.Should().Be(textData);
            zipArchiveArtifact.Bytes.Should().BeNull();

            zipArchiveArtifact = new ZipArchiveArtifact(doesNotExist, archive, archive.Entries.Last(), binaryExtensions);
            zipArchiveArtifact.IsBinary.Should().BeTrue();
            zipArchiveArtifact.Contents.Should().BeNull();
            zipArchiveArtifact.Bytes.Should().NotBeNull();
            zipArchiveArtifact.Bytes.Should().BeEquivalentTo(binaryData);
        }

        private (ZipArchive archive, ZipArchiveEntry entry) GetTextualArchiveData()
        {
            byte[] contents = Encoding.UTF8.GetBytes($"{Guid.NewGuid()}");
            ZipArchive archive = EnumeratedArtifactTests.CreateZipArchive("MyZippedFile.txt", contents);
            ZipArchiveEntry entry = archive.Entries.First();

            return (archive, entry);
        }

        private ZipArchive CreateMixedZipArchive(string textData, byte[] binaryData)
        {
            var stream = new MemoryStream();
            using (var populateArchive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                byte[] contents = Encoding.UTF8.GetBytes(textData);
                ZipArchiveEntry textEntry = populateArchive.CreateEntry("MyTextFile.txt", CompressionLevel.NoCompression);
                // textEntry.Open().Write(contents, 0, contents.Length);
                using (Stream zipEntryStream = textEntry.Open())
                {
                    zipEntryStream.Write(contents, 0, contents.Length);
                }

                ZipArchiveEntry binEntry = populateArchive.CreateEntry("MyBinFile.dll", CompressionLevel.NoCompression);
                using (Stream zipEntryStream = binEntry.Open())
                using (var zipBinaryFile = new BinaryWriter(zipEntryStream))
                {
                    zipBinaryFile.Write(binaryData);
                }
            }
            stream.Flush();
            stream.Position = 0;

            return new ZipArchive(stream, ZipArchiveMode.Read);
        }
    }
}
