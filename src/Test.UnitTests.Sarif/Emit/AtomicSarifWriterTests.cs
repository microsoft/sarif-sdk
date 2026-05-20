// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Emit;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Test.UnitTests.Emit
{
    public class AtomicSarifWriterTests : IDisposable
    {
        private readonly string _dir;

        public AtomicSarifWriterTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"atomic-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        [Fact]
        public void Write_CreatesDestinationFile()
        {
            string dest = Path.Combine(_dir, "out.sarif");

            AtomicSarifWriter.Write(dest, stream =>
            {
                byte[] bytes = Encoding.UTF8.GetBytes("hello");
                stream.Write(bytes, 0, bytes.Length);
            });

            File.Exists(dest).Should().BeTrue();
            File.ReadAllText(dest).Should().Be("hello");
        }

        [Fact]
        public void Write_OverwritesExistingDestinationFile()
        {
            string dest = Path.Combine(_dir, "out.sarif");
            File.WriteAllText(dest, "stale content");

            AtomicSarifWriter.Write(dest, stream =>
            {
                byte[] bytes = Encoding.UTF8.GetBytes("fresh");
                stream.Write(bytes, 0, bytes.Length);
            });

            File.ReadAllText(dest).Should().Be("fresh");
        }

        [Fact]
        public void Write_TolleratesConsumerDisposingStream()
        {
            // StreamWriter without leaveOpen will dispose the underlying FileStream; the writer
            // must accommodate that pattern (the bytes are already flushed to the OS).
            string dest = Path.Combine(_dir, "out.sarif");

            AtomicSarifWriter.Write(dest, stream =>
            {
                using var sw = new StreamWriter(stream, new UTF8Encoding(false));
                sw.Write("ok");
            });

            File.ReadAllText(dest).Should().Be("ok");
        }

        [Fact]
        public void Write_OnFailureDoesNotLeaveStagingTurd()
        {
            string dest = Path.Combine(_dir, "out.sarif");

            Action act = () => AtomicSarifWriter.Write(dest, _ => throw new InvalidOperationException("boom"));

            act.Should().Throw<InvalidOperationException>();
            // No .tmp leftover, no destination written.
            Directory.GetFiles(_dir).Should().BeEmpty();
            File.Exists(dest).Should().BeFalse();
        }

        [Fact]
        public void Write_StagesInSameDirectoryAsDestination()
        {
            string dest = Path.Combine(_dir, "out.sarif");
            string capturedStagingDir = null;

            AtomicSarifWriter.Write(dest, stream =>
            {
                // Find the staging file's directory while the write is in progress.
                string[] tmps = Directory.GetFiles(_dir, "*.tmp");
                capturedStagingDir = tmps.Length == 1 ? Path.GetDirectoryName(tmps[0]) : null;
                byte[] bytes = Encoding.UTF8.GetBytes("x");
                stream.Write(bytes, 0, bytes.Length);
            });

            capturedStagingDir.Should().Be(_dir);
        }

        [Fact]
        public void Write_CreatesParentDirectoryIfMissing()
        {
            string nested = Path.Combine(_dir, "a", "b", "out.sarif");

            AtomicSarifWriter.Write(nested, stream =>
            {
                byte[] bytes = Encoding.UTF8.GetBytes("nested");
                stream.Write(bytes, 0, bytes.Length);
            });

            File.ReadAllText(nested).Should().Be("nested");
        }
    }
}
