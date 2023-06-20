// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.IO.Compression;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Test.UnitTests.Sarif
{
    public class ArtifactProviderTests
    {

        [Fact]
        public void MultithreadedZipArchiveArtifactProvider_RetrieveSizeInBytes()
        {
            string entryContents = "test";
            ZipArchive zip = CreateZipArchive("test.zip", entryContents);
            var artifactProvider = new MultithreadedZipArchiveArtifactProvider(zip, FileSystem.Instance);
            foreach (IEnumeratedArtifact artifact in artifactProvider.Artifacts)
            {
                long? size = artifact.SizeInBytes;
                artifact.SizeInBytes.Should().Be(entryContents.Length);
            }
        }

        [Fact]
        public void MultithreadedZipArchiveArtifactProvider_SizeInBytesAndContentsAreAvailable()
        {
            string entryContents = "test";
            ZipArchive zip = CreateZipArchive("test.zip", entryContents);
            var artifactProvider = new MultithreadedZipArchiveArtifactProvider(zip, FileSystem.Instance);
            foreach (IEnumeratedArtifact artifact in artifactProvider.Artifacts)
            {
                artifact.SizeInBytes.Should().Be(entryContents.Length);
            }
        }

        private static ZipArchive CreateZipArchive(string fileName, string content)
        {
            var stream = new MemoryStream();
            using (var populateArchive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                ZipArchiveEntry entry = populateArchive.CreateEntry(fileName, CompressionLevel.NoCompression);
                using (var errorWriter = new StreamWriter(entry.Open()))
                {
                    errorWriter.Write(content);
                }
            }
            stream.Flush();
            stream.Position = 0;

            return new ZipArchive(stream, ZipArchiveMode.Read);
        }
    }
}
