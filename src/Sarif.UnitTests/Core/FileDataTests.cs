// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class FileDataTests
    {
        [Fact]
        public void FileData_Create_NullUri()
        {
            Action action = () => { FileData.Create(null, OptionallyEmittedData.None); };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FileData_ComputeHashes()
        {
            string filePath = Path.GetTempFileName();
            string fileContents = Guid.NewGuid().ToString();
            Uri uri = new Uri(filePath);

            try
            {
                File.WriteAllText(filePath, fileContents);
                FileData fileData = FileData.Create(uri, OptionallyEmittedData.Hashes);
                fileData.FileLocation.Should().Be(null);
                HashData hashes = HashUtilities.ComputeHashes(filePath);
                fileData.MimeType.Should().Be(MimeType.Binary);
                fileData.Contents.Should().BeNull();
                fileData.Hashes.Count.Should().Be(3);

                foreach (Hash hash in fileData.Hashes)
                {
                    switch (hash.Algorithm)
                    {
                        case "md5": { hash.Value.Should().Be(hashes.MD5); break; }
                        case "sha-1": { hash.Value.Should().Be(hashes.Sha1); break; }
                        case "sha-256": { hash.Value.Should().Be(hashes.Sha256); break; }
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
        public void FileData_PersistBinaryAndTextFileContents(
            string fileExtension,
            OptionallyEmittedData dataToInsert,
            bool shouldBePersisted)
        {
            string filePath = Path.GetTempFileName() + fileExtension;
            string fileContents = Guid.NewGuid().ToString();
            Uri uri = new Uri(filePath);

            try
            {
                File.WriteAllText(filePath, fileContents);
                FileData fileData = FileData.Create(uri, dataToInsert);
                fileData.FileLocation.Should().BeNull();

                if (dataToInsert.Includes(OptionallyEmittedData.Hashes))
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
                    fileData.Contents.Binary.Should().Be(encodedFileContents);
                    fileData.Contents.Text.Should().BeNull();
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
        public void FileData_PersistTextFileContentsBigEndianUnicode()
        {
            Encoding encoding = Encoding.BigEndianUnicode;
            string filePath = Path.GetTempFileName() + ".cs";
            string textValue = "अचम्भा";
            byte[] fileContents = encoding.GetBytes(textValue);

            Uri uri = new Uri(filePath);

            try
            {
                File.WriteAllBytes(filePath, fileContents);
                FileData fileData = FileData.Create(uri, OptionallyEmittedData.TextFiles, mimeType: null, encoding: encoding);
                fileData.FileLocation.Should().Be(null);
                fileData.MimeType.Should().Be(MimeType.CSharp);
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
        public void FileData_FileDoesNotExist()
        {
            // If a file does not exist, and we request file contents
            // persistence, the logger will not raise an exception
            string filePath = Path.GetTempFileName();
            Uri uri = new Uri(filePath);
            FileData fileData = FileData.Create(uri, OptionallyEmittedData.TextFiles);
            fileData.FileLocation.Should().Be(null);
            fileData.MimeType.Should().Be(MimeType.Binary);
            fileData.Hashes.Should().BeNull();
            fileData.Contents.Should().BeNull();
        }

        [Fact]
        public void FileData_FileIsLocked()
        {
            string filePath = Path.GetTempFileName();
            Uri uri = new Uri(filePath);

            try
            {
                // Place an exclusive read lock on file, so that FileData cannot access its contents.
                // This raises an IOException, which is swallowed by FileData.Create
                using (var exclusiveAccessReader = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    FileData fileData = FileData.Create(uri, OptionallyEmittedData.TextFiles);
                    fileData.FileLocation.Should().Be(null);
                    fileData.MimeType.Should().Be(MimeType.Binary);
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
        public void FileData_TextFileIsNotAccessibleDueToSecurity()
        {
            RunUnauthorizedAccessTextForFile(isTextFile: true);
        }

        [Fact]
        public void FileData_BinaryFileIsNotAccessibleDueToSecurity()
        {
            RunUnauthorizedAccessTextForFile(isTextFile: false);
        }

        [Fact]
        public void FileData_SerializeSingleFileRole()
        {
            FileData fileData = FileData.Create(new Uri("file:///foo.cs"), OptionallyEmittedData.None);
            fileData.Roles = FileRoles.AnalysisTarget;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };
            string result = JsonConvert.SerializeObject(fileData);

            result.Should().Be("{\"roles\":[\"analysisTarget\"],\"mimeType\":\"text/x-csharp\"}");
        }

        [Fact(Skip = "Broken codegen for Flags enums")]
        public void FileData_SerializeMultipleFileRoles()
        {
            FileData fileData = FileData.Create(new Uri("file:///foo.cs"), OptionallyEmittedData.None);
            fileData.Roles = FileRoles.ResponseFile | FileRoles.ResultFile;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };
            string actual = JsonConvert.SerializeObject(fileData);

            actual.Should().Be("{\"roles\":[\"responseFile\",\"resultFile\"],\"mimeType\":\"text/x-csharp\"}");
        }

        [Fact]
        public void FileData_DeserializeSingleFileRole()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };
            FileData actual = JsonConvert.DeserializeObject("{\"roles\":[\"analysisTarget\"],\"mimeType\":\"text/x-csharp\"}", typeof(FileData)) as FileData;
            actual.Roles.Should().Be(FileRoles.AnalysisTarget);
        }

        [Fact(Skip = "Broken codegen for Flags enums")]
        public void FileData_DeserializeMultipleFileRoles()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };
            FileData actual = JsonConvert.DeserializeObject("{\"roles\":[\"responseFile\",\"resultFile\"],\"mimeType\":\"text/x-csharp\"}", typeof(FileData)) as FileData;
            actual.Roles.Should().Be(FileRoles.ResponseFile | FileRoles.ResultFile);
        }

        private static void RunUnauthorizedAccessTextForFile(bool isTextFile)
        {
            string extension = isTextFile ? ".cs" : ".dll";
            string filePath = Path.GetFullPath(Guid.NewGuid().ToString()) + extension;
            Uri uri = new Uri(filePath);

            IFileSystem fileSystem = SetUnauthorizedAccessExceptionMock();

            FileData fileData = FileData.Create(
                uri,
                OptionallyEmittedData.TextFiles,
                mimeType: null,
                encoding: null,
                fileSystem: fileSystem);

            // We pass none here as the occurrence of UnauthorizedAccessException 
            // should result in non-population of any file contents.
            Validate(fileData, OptionallyEmittedData.None);
        }

        private static IFileSystem SetUnauthorizedAccessExceptionMock()
        {
            Mock<IFileSystem> mock = GetDefaultFileSystemMock();
            mock.Setup(fs => fs.ReadAllText(It.IsAny<string>(), It.IsAny<Encoding>())).Returns((string s, Encoding encoding) => { throw new UnauthorizedAccessException(); });
            mock.Setup(fs => fs.ReadAllBytes(It.IsAny<string>())).Returns((string s) => { throw new UnauthorizedAccessException(); });
            return mock.Object;
        }

        private static Mock<IFileSystem> GetDefaultFileSystemMock()
        {
            var mock = new Mock<IFileSystem>(MockBehavior.Strict);
            mock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns((string s) => { return true; });
            return mock;
        }

        private static void Validate(FileData fileData, OptionallyEmittedData dataToInsert)
        {
            if (dataToInsert.Includes(OptionallyEmittedData.TextFiles))
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