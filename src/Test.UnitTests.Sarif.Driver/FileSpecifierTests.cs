// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using FluentAssertions;

using Moq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class FileSpecifierTestsFixture : IDisposable
    {
        public FileSpecifierTestsFixture()
        {
            string rootDirectory = Guid.NewGuid().ToString();
            RootDirectory = Path.Combine(Path.GetTempPath(), rootDirectory);

            Directory.CreateDirectory(RootDirectory);

            for (int i = 0; i < 5; i++)
            {
                string childDirectory = Path.Combine(RootDirectory, "TestDirectory" + i.ToString());

                Directory.CreateDirectory(childDirectory);

                for (int j = 0; j < 5; j++)
                {
                    string file = Path.Combine(childDirectory, "TestFile" + j.ToString() + ".txt");
                    File.WriteAllText(file, "12345");
                }
            }
        }

        public string RootDirectory { get; set; }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, true);
            }
        }
    }

    [Collection("TestsUsingCurrentDirectory")]
    public class FileSpecifierTests : IClassFixture<FileSpecifierTestsFixture>
    {
        private readonly FileSpecifierTestsFixture _fixture;

        public FileSpecifierTests(FileSpecifierTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [ClassData(typeof(ResolveFilesTestData))]
        public void ResolveFiles(string input, int expectedCount)
        {
            // This test provides basic verification that simple patterns
            // matching one or more files in the current directory succeed
            var specifier = new FileSpecifier(input);
            specifier.Files.Count.Should().Be(expectedCount);

            specifier = new FileSpecifier(Path.Combine(Environment.CurrentDirectory, input));
            specifier.Files.Count.Should().Be(expectedCount);
        }

        [Theory]
        [ClassData(typeof(ResolveDirectoriesAndFilesTestData))]
        public void ResolveDirectoriesAndFiles(string input, bool recurse, int expectedCount)
        {
            string currentWorkingDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = _fixture.RootDirectory;

                // This test provides basic verification that simple patterns
                // matching one or more files in the current directory succeed
                var specifier = new FileSpecifier(input, recurse: recurse);
                specifier.Files.Count.Should().Be(expectedCount);

                specifier = new FileSpecifier(Path.Combine(_fixture.RootDirectory, input), recurse: recurse);
                specifier.Files.Count.Should().Be(expectedCount);

                Environment.CurrentDirectory = currentWorkingDirectory;
                specifier = new FileSpecifier(Path.Combine(_fixture.RootDirectory, input), recurse: recurse);
                specifier.Files.Count.Should().Be(expectedCount);
            }
            finally
            {
                Environment.CurrentDirectory = currentWorkingDirectory;
            }
        }

        [Fact]
        public void FileSpecifier_SkipsSymbolicLinkDirectoriesDuringRecursion()
        {
            var mockFileSystem = new Mock<IFileSystem>();

            string baseDir = Path.Combine(Path.GetTempPath(), "test");
            string targetDir = Path.Combine(baseDir, "target");
            string symlinkDir = Path.Combine(baseDir, "symlink");
            string targetFile = Path.Combine(targetDir, "test.txt");

            mockFileSystem.Setup(fs => fs.DirectoryExists(baseDir)).Returns(true);
            mockFileSystem.Setup(fs => fs.DirectoryExists(targetDir)).Returns(true);
            mockFileSystem.Setup(fs => fs.DirectoryExists(symlinkDir)).Returns(true);

            mockFileSystem.Setup(fs => fs.DirectoryGetDirectories(baseDir))
                          .Returns(new[] { targetDir, symlinkDir });

            mockFileSystem.Setup(fs => fs.DirectoryGetFiles(targetDir, "*.txt"))
                          .Returns(new[] { targetFile });
            mockFileSystem.Setup(fs => fs.DirectoryGetFiles(symlinkDir, "*.txt"))
                          .Returns(new[] { Path.Combine(symlinkDir, "test.txt") });

            mockFileSystem.Setup(fs => fs.IsSymbolicLink(targetDir)).Returns(false);
            mockFileSystem.Setup(fs => fs.IsSymbolicLink(symlinkDir)).Returns(true);

            var specifier = new FileSpecifier(Path.Combine(baseDir, "*.txt"), recurse: true, fileSystem: mockFileSystem.Object);
            IList<string> files = specifier.Files;

            files.Count.Should().Be(1);
            files[0].Should().Be(Path.GetFullPath(targetFile));
            files[0].Should().Contain("target");
            files[0].Should().NotContain("symlink");

            mockFileSystem.Verify(fs => fs.IsSymbolicLink(symlinkDir), Times.Once);
        }

        private class ResolveFilesTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "Sarif.dll", 1 };
                yield return new object[] { $".{Path.DirectorySeparatorChar}Sarif.dll", 1 };
                yield return new object[] { $".{Path.DirectorySeparatorChar}Sarif.dll*", 1 };
                yield return new object[] { "Sarif.dll*", 1 };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class ResolveDirectoriesAndFilesTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "TestFile1.txt", false, 0 };
                yield return new object[] { "TestFile1.txt", true, 5 };
                yield return new object[] { "DoesNotExist.dll", true, 0 };
                yield return new object[] { $".{Path.DirectorySeparatorChar}TestDirectory2{Path.DirectorySeparatorChar}TestFile3.txt", true, 1 };
                yield return new object[] { $".{Path.DirectorySeparatorChar}TestDirectory2{Path.DirectorySeparatorChar}TestFile3.txt", false, 1 };
                yield return new object[] { "*File*", true, 25 };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
