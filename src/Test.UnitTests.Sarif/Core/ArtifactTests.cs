// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Moq;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class ArtifactTests
    {
        [Fact]
        public void Artifact_Create_NullUri()
        {
            Action action = () => { Artifact.Create(null, OptionallyEmittedData.None); };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Artifact_ComputeHashes()
        {
            string filePath = Path.GetTempFileName();
            string fileContents = Guid.NewGuid().ToString();
            var uri = new Uri(filePath);

            try
            {
                File.WriteAllText(filePath, fileContents);
                var fileData = Artifact.Create(uri, OptionallyEmittedData.Hashes);
                fileData.Location.Should().Be(null);
                HashData hashes = HashUtilities.ComputeHashes(filePath);
                fileData.Contents.Should().BeNull();

                foreach (string algorithm in fileData.Hashes.Keys)
                {
                    switch (algorithm)
                    {
                        case "sha-1": { fileData.Hashes[algorithm].Should().Be(hashes.Sha1); break; }
                        case "sha-256": { fileData.Hashes[algorithm].Should().Be(hashes.Sha256); break; }
                        default: { true.Should().BeFalse(); break; /* unexpected algorithm kind */ }
                    }
                }
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        [Theory]
        // Unknown files are regarded as binary
        [InlineData(".unknown", OptionallyEmittedData.BinaryFiles, true)]
        [InlineData(".unknown", OptionallyEmittedData.TextFiles, false)]
        [InlineData(".exe", OptionallyEmittedData.BinaryFiles | OptionallyEmittedData.TextFiles, true)]
        [InlineData(".cs", OptionallyEmittedData.BinaryFiles | OptionallyEmittedData.TextFiles, true)]
        [InlineData(".jar", OptionallyEmittedData.BinaryFiles, true)]
        [InlineData(".jar", OptionallyEmittedData.TextFiles, false)]
        [InlineData(".cs", OptionallyEmittedData.BinaryFiles, false)]
        [InlineData(".cs", OptionallyEmittedData.TextFiles, true)]
        [InlineData(".h", ~OptionallyEmittedData.BinaryFiles, true)]
        [InlineData(".docx", ~OptionallyEmittedData.BinaryFiles, false)]
        [InlineData(".dll", ~OptionallyEmittedData.TextFiles, true)]
        [InlineData(".cpp", ~OptionallyEmittedData.TextFiles, false)]
        public void Artifact_PersistBinaryAndTextFileContents(
            string fileExtension,
            OptionallyEmittedData dataToInsert,
            bool shouldBePersisted)
        {
            string filePath = Path.GetTempFileName() + fileExtension;
            string fileContents = Guid.NewGuid().ToString();
            var uri = new Uri(filePath);

            try
            {
                File.WriteAllText(filePath, fileContents);
                var fileData = Artifact.Create(uri, dataToInsert);
                fileData.Location.Should().BeNull();

                if (dataToInsert.HasFlag(OptionallyEmittedData.Hashes))
                {
                    fileData.Hashes.Should().NotBeNull();
                }
                else
                {
                    fileData.Hashes.Should().BeNull();
                }

                string encodedFileContents = Convert.ToBase64String(File.ReadAllBytes(filePath));

                if (shouldBePersisted)
                {
                    string mimeType = MimeType.DetermineFromFileExtension(uri);
                    if (MimeType.IsBinaryMimeType(mimeType))
                    {
                        fileData.Contents.Binary.Should().Be(encodedFileContents);
                        fileData.Contents.Text.Should().BeNull();
                    }
                    else
                    {
                        fileData.Contents.Binary.Should().BeNull();
                        fileData.Contents.Text.Should().Be(fileContents);
                    }
                }
                else
                {
                    fileData.Contents.Should().BeNull();
                }
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        [Fact]
        public void Artifact_PersistTextFileContentsBigEndianUnicode()
        {
            Encoding encoding = Encoding.BigEndianUnicode;
            string filePath = Path.GetTempFileName() + ".cs";
            string textValue = "अचम्भा";
            byte[] fileContents = encoding.GetBytes(textValue);

            var uri = new Uri(filePath);

            try
            {
                File.WriteAllBytes(filePath, fileContents);
                var fileData = Artifact.Create(uri, OptionallyEmittedData.TextFiles, encoding: encoding);
                fileData.Location.Should().Be(null);
                fileData.Hashes.Should().BeNull();

                string encodedFileContents = encoding.GetString(fileContents);
                fileData.Contents.Text.Should().Be(encodedFileContents);
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        [Fact]
        public void Artifact_FileDoesNotExist()
        {
            // If a file does not exist, and we request file contents
            // persistence, the logger will not raise an exception
            string filePath = Path.GetTempFileName();
            var uri = new Uri(filePath);
            var fileData = Artifact.Create(uri, OptionallyEmittedData.TextFiles);
            fileData.Location.Should().Be(null);
            fileData.Hashes.Should().BeNull();
            fileData.Contents.Should().BeNull();
        }

        [Fact]
        public void Artifact_FileIsLocked()
        {
            string filePath = Path.GetTempFileName();
            var uri = new Uri(filePath);

            try
            {
                // Place an exclusive read lock on file, so that FileData cannot access its contents.
                // This raises an IOException, which is swallowed by FileData.Create
                using (FileStream exclusiveAccessReader = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    var fileData = Artifact.Create(uri, OptionallyEmittedData.TextFiles);
                    fileData.Location.Should().Be(null);
                    fileData.Hashes.Should().BeNull();
                    fileData.Contents.Should().BeNull();
                }
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        [Fact]
        public void Artifact_TextFileIsNotAccessibleDueToSecurity()
        {
            RunUnauthorizedAccessTextForFile(isTextFile: true);
        }

        [Fact]
        public void Artifact_BinaryFileIsNotAccessibleDueToSecurity()
        {
            RunUnauthorizedAccessTextForFile(isTextFile: false);
        }

        [Fact]
        public void Artifact_SerializeSingleFileRole()
        {
            var fileData = Artifact.Create(new Uri("file:///example.cs"), OptionallyEmittedData.None);
            fileData.Roles = ArtifactRoles.AnalysisTarget;

            string result = JsonConvert.SerializeObject(fileData);

            result.Should().Be("{\"roles\":[\"analysisTarget\"]}");
        }

        [Fact]
        public void Artifact_SerializeMultipleFileRoles()
        {
            var fileData = Artifact.Create(new Uri("file:///example.cs"), OptionallyEmittedData.None);
            fileData.Roles = ArtifactRoles.ResponseFile | ArtifactRoles.ResultFile;

            string actual = JsonConvert.SerializeObject(fileData);

            actual.Should().Be("{\"roles\":[\"responseFile\",\"resultFile\"]}");
        }

        [Fact]
        public void Artifact_DeserializeSingleFileRole()
        {
            var actual = JsonConvert.DeserializeObject("{\"roles\":[\"analysisTarget\"]}", typeof(Artifact)) as Artifact;
            actual.Roles.Should().Be(ArtifactRoles.AnalysisTarget);
        }

        [Fact]
        public void Artifact_DeserializeMultipleFileRoles()
        {
            var actual = JsonConvert.DeserializeObject("{\"roles\":[\"responseFile\",\"resultFile\"]}", typeof(Artifact)) as Artifact;
            actual.Roles.Should().Be(ArtifactRoles.ResponseFile | ArtifactRoles.ResultFile);
        }

        private static void RunUnauthorizedAccessTextForFile(bool isTextFile)
        {
            string extension = isTextFile ? ".cs" : ".dll";
            string filePath = Path.GetFullPath(Guid.NewGuid().ToString()) + extension;
            var uri = new Uri(filePath);

            IFileSystem fileSystem = SetUnauthorizedAccessExceptionMock();

            var fileData = Artifact.Create(
                uri,
                OptionallyEmittedData.TextFiles,
                encoding: null,
                fileSystem: fileSystem);

            // We pass none here as the occurrence of UnauthorizedAccessException 
            // should result in non-population of any file contents.
            Validate(fileData, OptionallyEmittedData.None);
        }

        private static IFileSystem SetUnauthorizedAccessExceptionMock()
        {
            Mock<IFileSystem> mock = GetDefaultFileSystemMock();
            mock.Setup(fs => fs.FileReadAllText(It.IsAny<string>(), It.IsAny<Encoding>())).Returns((string s, Encoding encoding) => { throw new UnauthorizedAccessException(); });
            mock.Setup(fs => fs.FileReadAllBytes(It.IsAny<string>())).Returns((string s) => { throw new UnauthorizedAccessException(); });
            return mock.Object;
        }

        private static Mock<IFileSystem> GetDefaultFileSystemMock()
        {
            var mock = new Mock<IFileSystem>(MockBehavior.Strict);
            mock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns((string s) => { return true; });
            return mock;
        }

        private static void Validate(Artifact fileData, OptionallyEmittedData dataToInsert)
        {
            if (dataToInsert.HasFlag(OptionallyEmittedData.TextFiles))
            {
                fileData.Contents.Should().NotBeNull();
            }
            else
            {
                fileData.Contents.Should().BeNull();
            }

        }
    }
}
