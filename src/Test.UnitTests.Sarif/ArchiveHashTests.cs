// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Test.UnitTests.Sarif
{
    public class ArchiveHashTests
    {
        [Fact]
        public void LogFileSkipped_InMemoryArchiveEntry_EmitsEntryHash()
        {
            byte[] entryBytes = { 1, 2, 3, 4, 5 };

            using var archiveStream = new MemoryStream();
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                ZipArchiveEntry entry = archive.CreateEntry("image.png");
                using Stream entryStream = entry.Open();
                entryStream.Write(entryBytes, 0, entryBytes.Length);
            }

            byte[] archiveBytes = archiveStream.ToArray();

            HashData archiveHashes;
            HashData entryHashes;

            using (var hashStream = new MemoryStream(archiveBytes))
            {
                archiveHashes = HashUtilities.ComputeHashes(hashStream);
            }

            using (var hashStream = new MemoryStream(entryBytes))
            {
                entryHashes = HashUtilities.ComputeHashes(hashStream);
            }

            archiveHashes.Sha256.Should().NotBe(entryHashes.Sha256);

            using var readStream = new MemoryStream(archiveBytes);
            using var readArchive = new ZipArchive(readStream, ZipArchiveMode.Read);
            var artifact = new ZipArchiveArtifact(
                new Uri("file:///C:/in-memory.zip"),
                readArchive,
                readArchive.Entries.Single());

            using var logger = new MemoryStreamSarifLogger(
                dataToInsert: OptionallyEmittedData.Hashes,
                levels: BaseLogger.ErrorWarningNote,
                kinds: BaseLogger.Fail);

            var context = new TestContext
            {
                DataToInsert = OptionallyEmittedData.Hashes,
                Logger = logger,
            };

            Notes.LogFileSkipped(context, artifact, "denied by test policy");

            var sarifLog = logger.ToSarifLog();
            Artifact sarifArtifact = sarifLog.Runs[0].Artifacts.Single();

            sarifArtifact.Hashes["sha-256"].Should().Be(entryHashes.Sha256);
            sarifArtifact.Hashes["sha-256"].Should().NotBe(archiveHashes.Sha256);
        }

        private sealed class TestContext : AnalyzeContextBase
        {
        }
    }
}
