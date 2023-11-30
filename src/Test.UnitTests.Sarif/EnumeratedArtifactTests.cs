// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Driver;

using Moq;

using Xunit;

namespace Test.UnitTests.Sarif
{
    public class EnumeratedArtifactTests
    {
        [Fact]
        public void EnumeratedArtifact_TextFileEncodingsAreProperlyDetected()
        {
            // This test is just a placeholder. We do not have logic below
            // that reliably produces disk files with appropriate BOMs/file
            // formats to properly test the encoding detection from the new
            // helpers we have added. This test passes but is very incomplete.
            // If/when we have additional demands to support esoteric encodings
            // in analysis, we can build this out. Currently, things are not
            // guaranteed to work reliably except in ASCII, UTF8, Unicode.

            string guid = Guid.NewGuid().ToString();

#pragma warning disable SYSLIB0001 // Type or member is obsolete
            foreach (Encoding encoding in new[] { Encoding.UTF8 })
            {
                byte[] bytes = encoding.GetBytes(guid);

                using var tempFile = new TempFile();
                File.WriteAllBytes(tempFile.Name, bytes);

                using var stream = new MemoryStream(File.ReadAllBytes(tempFile.Name));

                var fileSystem = new Mock<IFileSystem>();
                fileSystem
                    .Setup(f => f.FileExists(It.IsAny<string>()))
                    .Returns(true);

                fileSystem
                    .Setup(f => f.FileOpenRead(It.IsAny<string>()))
                    .Returns(stream);

                var artifact = new EnumeratedArtifact(fileSystem.Object)
                {
                    Uri = new Uri(@"c:\SomeFile.txt"),
                    Stream = stream,
                };

                artifact.Encoding.Should().Be(encoding);
            }
#pragma warning restore SYSLIB0001 // Type or member is obsolete
        }

        [Fact]
        public void EnumeratedArtifact_BinaryFile_OnDisk()
        {
            string filePath = this.GetType().Assembly.Location;

            var artifact = new EnumeratedArtifact(new FileSystem())
            {
                Uri = new Uri(filePath)
            };

            int fileSize = (int)new FileInfo(filePath).Length;

            ValidateBinaryArtifact(artifact, fileSize);
        }

        [Fact]
        public void EnumeratedArtifact_TextFile_OnDisk()
        {
            using var tempFile = new TempFile();
            string filePath = tempFile.Name;

            File.WriteAllText(filePath, $"{Guid.NewGuid}");

            var artifact = new EnumeratedArtifact(new FileSystem())
            {
                Uri = new Uri(filePath)
            };

            int fileSize = (int)new FileInfo(filePath).Length;

            ValidateTextArtifact(artifact, fileSize);
        }

        [Fact]
        public void EnumeratedArtifact_BinaryFile_SeekableStream()
        {
            string filePath = this.GetType().Assembly.Location;

            // To ensure that this test operates strictly against the
            // stream we provider, we will force the file system to
            // return null if the object attempts to load anything
            // from disk.
            var fileSystem = new Mock<IFileSystem>();
            fileSystem
                .Setup(f => f.FileOpenRead(It.IsAny<string>()))
                .Returns((Stream)null);

            var artifact = new EnumeratedArtifact(fileSystem.Object)
            {
                Uri = new Uri(filePath),
                Stream = new MemoryStream(File.ReadAllBytes(filePath)),
            };

            int fileSize = (int)new FileInfo(filePath).Length;

            ValidateBinaryArtifact(artifact, fileSize);
        }

        [Fact]
        public void EnumeratedArtifact_TextFile_SeekableStream()
        {
            using var tempFile = new TempFile();
            string filePath = tempFile.Name;

            File.WriteAllText(filePath, $"{Guid.NewGuid}");

            var artifact = new EnumeratedArtifact(new FileSystem())
            {
                Uri = new Uri(filePath),
                Stream = new MemoryStream(File.ReadAllBytes(filePath)),
            };

            int fileSize = (int)new FileInfo(filePath).Length;

            // Ensure that we don't have a file on disk and that the
            // enumerated artifact is operating strictly from the stream.
            tempFile.Dispose();
            File.Exists(filePath).Should().BeFalse();

            ValidateTextArtifact(artifact, fileSize);
        }

        [Fact]
        public void EnumeratedArtifact_TextFile_NonSeekableStream()
        {
            string guid = $"{Guid.NewGuid()}";
            ZipArchive archive = CreateZipArchive("MyTextualFile.txt", Encoding.UTF8.GetBytes(guid));

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                var artifact = new EnumeratedArtifact(FileSystem.Instance)
                {
                    Uri = new Uri(entry.FullName, UriKind.RelativeOrAbsolute),
                    SupportNonSeekableStreams = true,
                    Stream = entry.Open(),
                };

                ValidateTextArtifact(artifact, guid.Length);
            }
        }

        [Fact]
        public void EnumeratedArtifact_BinaryFile_NonSeekableStream()
        {
            string filePath = this.GetType().Assembly.Location;
            using FileStream reader = File.OpenRead(filePath);

            int headerSize = 1024;
            byte[] data = new byte[headerSize];
            reader.Read(data, 0, data.Length);

            ZipArchive archive = CreateZipArchive("MyBinaryFile.dll", data);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                var artifact = new EnumeratedArtifact(FileSystem.Instance)
                {
                    Uri = new Uri(entry.FullName, UriKind.RelativeOrAbsolute),
                    SupportNonSeekableStreams = true,
                    Stream = entry.Open(),
                };

                ValidateBinaryArtifact(artifact, headerSize);
            }
        }

        [Fact]
        public void EnumeratedArtifact_TextFile_SizeInBytesWithNoUriReturnsNull()
        {
            var artifact = new EnumeratedArtifact(new FileSystem())
            {
                Contents = $"{Guid.NewGuid}",
            };

            artifact.SizeInBytes.Should().BeNull();
        }

        [Fact]
        public void EnumeratedArtifact_TextFile_NonSeekableStreamRaiseExceptions()
        {
            string guid = $"{Guid.NewGuid()}";
            ZipArchive archive = CreateZipArchive("MyTextualFile.txt", Encoding.UTF8.GetBytes(guid));

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                var artifact = new EnumeratedArtifact(FileSystem.Instance)
                {
                    Stream = entry.Open(),
                };

                Assert.Throws<InvalidOperationException>(() => ValidateTextArtifact(artifact, guid.Length));
            }
        }

        private void ValidateBinaryArtifact(EnumeratedArtifact artifact, int sizeInBytes)
        {
            artifact.Bytes.Should().NotBeNull();
            artifact.Bytes.Length.Should().Be(sizeInBytes);
            artifact.SizeInBytes.Should().Be(sizeInBytes);

            artifact.Contents.Should().BeNull();
        }

        private void ValidateTextArtifact(EnumeratedArtifact artifact, int sizeInBytes)
        {
            artifact.Contents.Should().NotBeNull();
            artifact.Contents.Length.Should().Be(sizeInBytes);
            artifact.SizeInBytes.Should().Be(sizeInBytes);

            artifact.Bytes.Should().BeNull();
        }


        [Fact]
        public void EnumeratedArtifact_FileSizeShouldNotFaultInContentsFromDisk()
        {
            string filePath = this.GetType().Assembly.Location;
            var enumeratedArtifact = new EnumeratedArtifact(FileSystem.Instance)
            {
                Uri = new Uri(filePath)
            };

            long fileLength = new FileInfo(filePath).Length;
            enumeratedArtifact.SizeInBytes.Should().Be((long)fileLength);

            enumeratedArtifact.contents.Should().BeNull();
            enumeratedArtifact.Stream.Should().BeNull();
        }

        [Fact]
        public void EnumeratedArtifact_FileSizeReturnedForInMemoryContents()
        {
            string filePath = this.GetType().Assembly.Location;
            string contents = "Test content.";

            var enumeratedArtifact = new EnumeratedArtifact(FileSystem.Instance)
            {
                Uri = new Uri(filePath),
                Contents = contents,
            };

            enumeratedArtifact.contents.Should().Be(contents);
            enumeratedArtifact.SizeInBytes.Should().Be((long)contents.Length);
        }

        [Fact]
        public void EnumeratedArtifact_FileSizeReturnedForInMemoryStream()
        {
            string filePath = this.GetType().Assembly.Location;
            string contents = "Test content.";
            byte[] contentBytes = Encoding.UTF8.GetBytes(contents);

            var stream = new MemoryStream();
            stream.Write(contentBytes, 0, contentBytes.Length);

            var enumeratedArtifact = new EnumeratedArtifact(FileSystem.Instance)
            {
                Uri = new Uri(filePath),
                Stream = stream,
            };

            enumeratedArtifact.contents.Should().BeNull();
            enumeratedArtifact.SizeInBytes.Should().Be((long)contentBytes.Length);
            enumeratedArtifact.Contents.Should().Be(contents);
        }

        private static ZipArchive CreateZipArchive(string fileName, byte[] content)
        {
            var stream = new MemoryStream();
            using (var populateArchive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                ZipArchiveEntry entry = populateArchive.CreateEntry(fileName, CompressionLevel.NoCompression);
                entry.Open().Write(content, 0, content.Length);
            }
            stream.Flush();
            stream.Position = 0;

            return new ZipArchive(stream, ZipArchiveMode.Read);
        }
    }
}
