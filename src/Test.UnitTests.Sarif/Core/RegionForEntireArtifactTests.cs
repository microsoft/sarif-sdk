// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Pins <see cref="Region.ForEntireArtifact(string)"/> and
    /// <see cref="Region.ForEntireArtifact(long)"/> — the SDK helpers for
    /// constructing a binary <see cref="Region"/> covering an entire artifact.
    /// </summary>
    public class RegionForEntireArtifactTests : IDisposable
    {
        private readonly string _scratchDir;

        public RegionForEntireArtifactTests()
        {
            this._scratchDir = Path.Combine(Path.GetTempPath(), "region-for-entire-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this._scratchDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(this._scratchDir))
                {
                    Directory.Delete(this._scratchDir, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup.
            }

            GC.SuppressFinalize(this);
        }

        [Fact]
        public void ForEntireArtifact_FromPath_ProducesBinaryRegion_OverWholeFile()
        {
            string path = Path.Combine(this._scratchDir, "small.bin");
            byte[] payload = new byte[1234];
            File.WriteAllBytes(path, payload);

            Region region = Region.ForEntireArtifact(path);

            region.ByteOffset.Should().Be(0);
            region.ByteLength.Should().Be(1234);
            region.IsBinaryRegion.Should().BeTrue();
            region.IsLineColumnBasedTextRegion.Should().BeFalse(
                "the helper produces a binary region; consumers see 'whole artifact' unambiguously");
            region.IsOffsetBasedTextRegion.Should().BeFalse();
        }

        [Fact]
        public void ForEntireArtifact_FromPath_HandlesEmptyFile()
        {
            string path = Path.Combine(this._scratchDir, "empty.bin");
            File.WriteAllBytes(path, Array.Empty<byte>());

            Region region = Region.ForEntireArtifact(path);

            region.ByteOffset.Should().Be(0);
            region.ByteLength.Should().Be(0);
            region.IsBinaryRegion.Should().BeTrue();
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(1024L)]
        [InlineData(int.MaxValue - 1L)]
        public void ForEntireArtifact_FromLength_RoundTripsExactly(long size)
        {
            Region region = Region.ForEntireArtifact(size);
            region.ByteOffset.Should().Be(0);
            region.ByteLength.Should().Be((int)size);
            region.IsBinaryRegion.Should().BeTrue();
        }

        [Fact]
        public void ForEntireArtifact_FromLength_ClampsAboveIntMaxValue()
        {
            // SARIF byteLength is an int (§3.30.10); larger artifacts can't be
            // expressed as a single region. Clamp rather than throw so callers
            // with huge artifacts still get a syntactically valid region.
            Region region = Region.ForEntireArtifact((long)int.MaxValue + 1);
            region.ByteLength.Should().Be(int.MaxValue);
        }

        [Fact]
        public void ForEntireArtifact_FromLength_RejectsNegative()
        {
            Action act = () => Region.ForEntireArtifact(-1L);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ForEntireArtifact_FromPath_RejectsEmpty(string path)
        {
            Action act = () => Region.ForEntireArtifact(path);
            act.Should().Throw<ArgumentException>();
        }
    }
}
