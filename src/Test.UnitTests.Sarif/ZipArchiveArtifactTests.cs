// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [Fact]
        public void ZipArchiveArtifact_StreamThrowsForTextEntry()
        {
            byte[] contents = Encoding.UTF8.GetBytes($"text-{Guid.NewGuid()}");
            ZipArchive archive = EnumeratedArtifactTests.CreateZipArchive("entry.txt", contents);
            var artifact = new ZipArchiveArtifact(new Uri("file://does-not-exist.zip"), archive, archive.Entries.First());

            artifact.IsBinary.Should().BeFalse();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => artifact.Stream);
            exception.Message.Should().Contain("Contents");
        }

        [Fact]
        public void ZipArchiveArtifact_StreamIsByteExactForBinaryEntry()
        {
            byte[] contents = new byte[2048];
            new Random(42).NextBytes(contents);
            contents[0] = 0x00;

            ZipArchive archive = EnumeratedArtifactTests.CreateZipArchive("entry.bin", contents);
            var artifact = new ZipArchiveArtifact(new Uri("file://does-not-exist.zip"), archive, archive.Entries.First());

            artifact.IsBinary.Should().BeTrue();
            ReadFully(artifact.Stream).Should().Equal(contents);
        }

        [Fact]
        public void ZipArchiveArtifact_StreamThrowsForTextEntryRegardlessOfAccessOrder()
        {
            byte[] contents = Encoding.UTF8.GetBytes($"text-{Guid.NewGuid()}");
            ZipArchive archive = EnumeratedArtifactTests.CreateZipArchive("entry.txt", contents);
            var artifact = new ZipArchiveArtifact(new Uri("file://does-not-exist.zip"), archive, archive.Entries.First());

            // Faulting in the textual content must not change the Stream
            // contract: a raw byte stream is never synthesized for a textual
            // entry, and the refusal is deterministic, not call-order sensitive.
            _ = artifact.Contents;

            Assert.Throws<InvalidOperationException>(() => artifact.Stream);
        }

        [Fact]
        public void ZipArchiveArtifact_StreamReturnsIndependentSnapshotPerAccess()
        {
            byte[] contents = new byte[1024];
            new Random(7).NextBytes(contents);
            contents[0] = 0x00;

            ZipArchive archive = EnumeratedArtifactTests.CreateZipArchive("entry.bin", contents);
            var artifact = new ZipArchiveArtifact(new Uri("file://does-not-exist.zip"), archive, archive.Entries.First());

            artifact.IsBinary.Should().BeTrue();

            Stream first = artifact.Stream;
            Stream second = artifact.Stream;

            ReferenceEquals(first, second).Should().BeFalse();

            first.ReadByte();
            second.Position.Should().Be(0);
        }

        [Fact]
        public void ZipArchiveArtifact_StreamIsThreadSafeAcrossEntriesOfOneArchive()
        {
            var expected = new Dictionary<string, byte[]>();
            var random = new Random(11);
            for (int i = 0; i < 64; i++)
            {
                byte[] data = new byte[2048];
                random.NextBytes(data);
                data[0] = 0x00;
                expected[$"dir/entry-{i}.bin"] = data;
            }

            ZipArchive archive = CreateMultiEntryArchive(expected);
            var provider = new MultithreadedZipArchiveArtifactProvider(
                new Uri("file://does-not-exist.zip"), archive, FileSystem.Instance);
            List<IEnumeratedArtifact> artifacts = provider.Artifacts.ToList();

            var streamed = new ConcurrentBag<string>();
            Parallel.ForEach(
                artifacts,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                artifact => streamed.Add(Convert.ToBase64String(ReadFully(artifact.Stream))));

            IEnumerable<string> want = expected.Values
                .Select(Convert.ToBase64String)
                .OrderBy(s => s, StringComparer.Ordinal);

            streamed.OrderBy(s => s, StringComparer.Ordinal).Should().Equal(want);
        }

        private static byte[] ReadFully(Stream stream)
        {
            using (stream)
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static ZipArchive CreateMultiEntryArchive(IDictionary<string, byte[]> entries)
        {
            var stream = new MemoryStream();
            using (var populateArchive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (KeyValuePair<string, byte[]> entry in entries)
                {
                    ZipArchiveEntry archiveEntry = populateArchive.CreateEntry(entry.Key, CompressionLevel.Optimal);
                    using Stream entryStream = archiveEntry.Open();
                    entryStream.Write(entry.Value, 0, entry.Value.Length);
                }
            }
            stream.Flush();
            stream.Position = 0;

            return new ZipArchive(stream, ZipArchiveMode.Read);
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
