// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class FileDataTests
    {
        [Fact]
        public void FileData_Create_NullUri()
        {
            Action action = () => { FileData.Create(null, LoggingOptions.None); };

            action.ShouldThrow<ArgumentNullException>();
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
                FileData fileData = FileData.Create(uri, LoggingOptions.ComputeFileHashes);
                fileData.Uri.Should().Be(uri.ToString());
                HashData hashes = HashUtilities.ComputeHashes(filePath);
                fileData.Hashes.Count.Should().Be(3);

                foreach (Hash hash in fileData.Hashes)
                {
                    switch (hash.Algorithm)
                    {
                        case AlgorithmKind.MD5: { hash.Value.Should().Be(hashes.MD5); break; }
                        case AlgorithmKind.Sha1: { hash.Value.Should().Be(hashes.Sha1); break; }
                        case AlgorithmKind.Sha256: { hash.Value.Should().Be(hashes.Sha256); break; }
                    }
                }
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        [Fact]
        public void FileData_PersistFileContents()
        {
            string filePath = Path.GetTempFileName();
            string fileContents = Guid.NewGuid().ToString();
            Uri uri = new Uri(filePath);

            try
            {
                File.WriteAllText(filePath, fileContents);
                FileData fileData = FileData.Create(uri, LoggingOptions.PersistFileContents);
                fileData.Uri.Should().Be(uri.ToString());
                fileData.Hashes.Should().BeNull();

                string encodedFileContents = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContents));
                fileData.Contents.Should().Be(encodedFileContents);
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
            FileData fileData = FileData.Create(uri, LoggingOptions.PersistFileContents);
            fileData.Uri.Should().Be(uri.ToString());
            fileData.Hashes.Should().BeNull();
            fileData.Contents.Should().Be(String.Empty);
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
                    FileData fileData = FileData.Create(uri, LoggingOptions.PersistFileContents);
                    fileData.Uri.Should().Be(uri.ToString());
                    fileData.Hashes.Should().BeNull();
                    fileData.Contents.Should().BeNull();
                }
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }
    }
}