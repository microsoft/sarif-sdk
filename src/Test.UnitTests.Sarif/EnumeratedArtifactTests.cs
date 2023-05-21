// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Test.UnitTests.Sarif
{
    public class EnumeratedArtifactTests
    {
        [Fact]
        public void EnumeratedArtifact_FileSizeShouldNotFaultInContents()
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
    }
}
