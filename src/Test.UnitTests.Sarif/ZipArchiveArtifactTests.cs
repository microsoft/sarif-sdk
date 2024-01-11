// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO.Compression;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Test.UnitTests.Sarif
{
    public class ZipArchiveArtifactTests
    {
        [Fact]
        public void ZipArchiveArtifact_BytesAndContentsCannotBeSet()
        {
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextArchiveData();
            var zipArchiveArtifact = new ZipArchiveArtifact(testData.archive, testData.entry);

            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Contents = string.Empty);
            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Bytes = new byte[] { });
        }

        [Fact]
        public void ZipArchiveArtifact_EncodingNotImplemented()
        {
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextArchiveData();
            var zipArchiveArtifact = new ZipArchiveArtifact(testData.archive, testData.entry);

            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Encoding);
            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Encoding = Encoding.UTF8);
        }

        [Fact]
        public void ZipArchiveArtifact_SettingStreamNotImplemented()
        {
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextArchiveData();
            var zipArchiveArtifact = new ZipArchiveArtifact(testData.archive, testData.entry);

            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.Stream = null);
        }

        [Fact]
        public void ZipArchiveArtifact_SettingSizeInBytesNotImplemented()
        {
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextArchiveData();
            var zipArchiveArtifact = new ZipArchiveArtifact(testData.archive, testData.entry);

            Assert.Throws<NotImplementedException>(() => zipArchiveArtifact.SizeInBytes = null);
        }

        [Fact]
        public void ZipArchiveArtifact_NonNullArchiveAndEntryAreRequired()
        {
            (ZipArchive archive, ZipArchiveEntry entry) testData = GetTextArchiveData();

            Assert.Throws<ArgumentNullException>(() => new ZipArchiveArtifact(archive: null, testData.entry));
            Assert.Throws<ArgumentNullException>(() => new ZipArchiveArtifact(testData.archive, entry: null));
        }


        private (ZipArchive archive, ZipArchiveEntry entry) GetTextArchiveData()
        {
            byte[] contents = Encoding.UTF8.GetBytes($"{Guid.NewGuid()}");
            ZipArchive archive = EnumeratedArtifactTests.CreateZipArchive("MyZippedFile.txt", contents);
            ZipArchiveEntry entry = archive.Entries.First();

            return (archive, entry);
        }
    }
}
