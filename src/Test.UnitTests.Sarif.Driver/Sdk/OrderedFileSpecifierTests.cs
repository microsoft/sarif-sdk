// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using FluentAssertions;

using Moq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class OrderedFileSpecifierTestsFixture : IDisposable
    {
        /// <summary>
        /// Fixture that creates a temporary root directory with a set of subdirectories and files.
        /// The structure includes 5 subdirectories (TestDirectory0...TestDirectory4), each containing 5 text files.
        /// This setup simulates a realistic file system structure for testing file enumeration logic.
        /// </summary>
        public OrderedFileSpecifierTestsFixture()
        {
            TempPath = Path.GetTempPath();
            RootDirectoryRelativePath = Guid.NewGuid().ToString();
            RootDirectory = Path.Combine(TempPath, RootDirectoryRelativePath);

            Directory.CreateDirectory(RootDirectory);

            for (int i = 0; i < 5; i++)
            {
                string childDirectory = Path.Combine(RootDirectory, "OrderedFileSpecifierTestDirectory" + i.ToString());

                Directory.CreateDirectory(childDirectory);

                for (int j = 0; j < 5; j++)
                {
                    string file = Path.Combine(childDirectory, "TestFile" + j.ToString() + ".txt");
                    File.WriteAllText(file, "12345");
                }
            }
        }

        public string RootDirectory { get; set; }

        public string RootDirectoryRelativePath { get; set; }

        public string TempPath { get; set; }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, true);
            }
        }
    }

    [Collection("TestsUsingCurrentDirectory")]
    public class OrderedFileSpecifierTests : IClassFixture<OrderedFileSpecifierTestsFixture>
    {
        private readonly OrderedFileSpecifierTestsFixture _fixture;

        public OrderedFileSpecifierTests(OrderedFileSpecifierTestsFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// This test creates a wide range of high-level command-line argument patterns to verify that the enumerated artifacts 
        /// are generated correctly when using either absolute or relative paths from the OrderedFileSpecifierFixture.
        /// </summary>
        [Fact]
        public void ResolveFileAndDirectoryPaths()
        {
            var testCases = new List<(string, int)>
            {
                // Absolute directory paths.
                (_fixture.RootDirectory, 25),
                (_fixture.RootDirectory + "/", 25),
                (_fixture.RootDirectory + "/*", 25),
                (_fixture.RootDirectory + "/*.txt", 25),
                (_fixture.RootDirectory + "/Test*.txt", 25),
                (Path.Combine(_fixture.RootDirectory, "OrderedFileSpecifierTestDirectory0"), 5),
                (Path.Combine(_fixture.RootDirectory, "OrderedFileSpecifierTestDirectory0/*.txt"), 5),
                (Path.Combine(_fixture.RootDirectory, "OrderedFileSpecifierTestDirectory0/Test*.txt"), 5),

                // Relative directory paths.
                (_fixture.RootDirectoryRelativePath, 25),
                (_fixture.RootDirectoryRelativePath + "/", 25),
                (_fixture.RootDirectoryRelativePath + "/*", 25),
                (_fixture.RootDirectoryRelativePath + "/*.txt", 25),
                (_fixture.RootDirectoryRelativePath + "/Test*.txt", 25),
                ("./" + _fixture.RootDirectoryRelativePath, 25),
                ("./" + _fixture.RootDirectoryRelativePath + "/", 25),
                (Path.Combine(_fixture.RootDirectoryRelativePath, "OrderedFileSpecifierTestDirectory0"), 5),
                (Path.Combine(_fixture.RootDirectoryRelativePath, "OrderedFileSpecifierTestDirectory0/*.txt"), 5),
                (Path.Combine(_fixture.RootDirectoryRelativePath, "OrderedFileSpecifierTestDirectory0/Test*.txt"), 5),

                 // Absolute file paths.
                (_fixture.RootDirectory + "/OrderedFileSpecifierTestDirectory0/" + "TestFile0.txt", 1),
                (_fixture.RootDirectory + "/OrderedFileSpecifierTestDirectory1/" + "TestFile1.txt", 1),

                 // Relative file paths.
                (_fixture.RootDirectoryRelativePath + "/OrderedFileSpecifierTestDirectory0/" + "TestFile0.txt", 1),
                ("./" + _fixture.RootDirectoryRelativePath + "/OrderedFileSpecifierTestDirectory0/" + "TestFile0.txt", 1),

                // Manually tested for symbolic links.
            };

            var testCasesWindowsOnly = new List<(string, int)>
            {
                (_fixture.RootDirectory + "\\", 25),
                (_fixture.RootDirectory + "\\*", 25),
                (_fixture.RootDirectoryRelativePath + "\\", 25),
                (_fixture.RootDirectoryRelativePath + "\\*", 25),
                (".\\" + _fixture.RootDirectoryRelativePath, 25),
                (".\\" + _fixture.RootDirectoryRelativePath + "\\", 25),
                (_fixture.RootDirectory + "\\OrderedFileSpecifierTestDirectory0\\" + "TestFile0.txt", 1),
                (_fixture.RootDirectoryRelativePath + "\\OrderedFileSpecifierTestDirectory0\\" + "TestFile0.txt", 1),
                ("./" + _fixture.RootDirectoryRelativePath + "\\OrderedFileSpecifierTestDirectory0\\" + "TestFile0.txt", 1)
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                testCases.AddRange(testCasesWindowsOnly);
            }

            var sb = new StringBuilder();

            foreach ((string, int) testCase in testCases)
            {
                string currentWorkingDirectory = Environment.CurrentDirectory;
                try
                {
                    // Set current directory to the parent of _fixture.RootDirectory, that is, the assembly directory
                    // to avoid non-deterministic resolution of relative paths when the current working directory changes.
                    Environment.CurrentDirectory = _fixture.TempPath;

                    var specifier = new OrderedFileSpecifier(testCase.Item1, recurse: true);
                    int artifactCount = specifier.Artifacts.Count();

                    if (!Equals(artifactCount, testCase.Item2))
                    {
                        sb.AppendFormat("Incorrect count of artifacts enumerated for specifier {0}. Expected '{1}' but saw '{2}'.", testCase.Item1, testCase.Item2, artifactCount).AppendLine();
                    }
                }
                finally
                {
                    Environment.CurrentDirectory = currentWorkingDirectory;
                }
            }

            sb.Length.Should().Be(0, because: "all artifacts should be enumerated but the following cases failed." + Environment.NewLine + sb.ToString());
        }

        [Fact]
        public void OrderedFileSpecifier_SkipsSymbolicLinkDirectoriesDuringRecursion()
        {
            var mockFileSystem = new Mock<IFileSystem>();

            string baseDir = Path.Combine(Path.GetTempPath(), "test");
            string targetDir = Path.Combine(baseDir, "target");
            string symlinkDir = Path.Combine(baseDir, "symlink");
            string targetFile = Path.Combine(targetDir, "test.txt");

            mockFileSystem.Setup(fs => fs.DirectoryExists(baseDir)).Returns(true);
            mockFileSystem.Setup(fs => fs.DirectoryExists(targetDir)).Returns(true);
            mockFileSystem.Setup(fs => fs.DirectoryExists(symlinkDir)).Returns(true);

            mockFileSystem.Setup(fs => fs.DirectoryEnumerateDirectories(baseDir, "*", SearchOption.TopDirectoryOnly))
                          .Returns(new[] { targetDir, symlinkDir });

            mockFileSystem.Setup(fs => fs.DirectoryEnumerateFiles(targetDir, "*.txt", SearchOption.TopDirectoryOnly))
                          .Returns(new[] { targetFile });
            mockFileSystem.Setup(fs => fs.DirectoryEnumerateFiles(symlinkDir, "*.txt", SearchOption.TopDirectoryOnly))
                          .Returns(new[] { Path.Combine(symlinkDir, "test.txt") });

            mockFileSystem.Setup(fs => fs.IsSymbolicLink(targetDir)).Returns(false);
            mockFileSystem.Setup(fs => fs.IsSymbolicLink(symlinkDir)).Returns(true);

            var specifier = new OrderedFileSpecifier(Path.Combine(baseDir, "*.txt"), recurse: true, fileSystem: mockFileSystem.Object);
            var artifacts = specifier.Artifacts.ToList();

            artifacts.Count.Should().Be(1);
            artifacts[0].Uri.LocalPath.Should().Be(targetFile);
            artifacts[0].Uri.LocalPath.Should().Contain("target");
            artifacts[0].Uri.LocalPath.Should().NotContain("symlink");

            mockFileSystem.Verify(fs => fs.IsSymbolicLink(symlinkDir), Times.Once);
        }

        [Fact]
        public void OrderedFileSpecifier_SkipsRealSymbolicLinkDirectoriesDuringRecursion()
        {
            string tempFolder = Path.GetTempPath();
            string testBaseDir = Path.Combine(tempFolder, Guid.NewGuid().ToString());
            string targetDir = Path.Combine(testBaseDir, "realTarget");
            string symlinkDir = Path.Combine(testBaseDir, "realSymlink");
            string targetFile1 = Path.Combine(targetDir, "target1.txt");
            string targetFile2 = Path.Combine(targetDir, "target2.txt");

            try
            {
                Directory.CreateDirectory(testBaseDir);
                Directory.CreateDirectory(targetDir);
                File.WriteAllText(targetFile1, "This is target file 1");
                File.WriteAllText(targetFile2, "This is target file 2");

                bool createdSymlink = TryCreateSymbolicLink(symlinkDir, targetDir, isDirectory: true);
                createdSymlink.Should().BeTrue("symbolic link creation should succeed for this test to be valid");

                Directory.Exists(symlinkDir).Should().BeTrue();
                var fileSystem = new FileSystem();
                fileSystem.IsSymbolicLink(symlinkDir).Should().BeTrue();
                fileSystem.IsSymbolicLink(targetDir).Should().BeFalse();

                var specifier = new OrderedFileSpecifier(Path.Combine(testBaseDir, "*.txt"), recurse: true);
                var artifacts = specifier.Artifacts.ToList();

                artifacts.Count.Should().Be(2);
                
                var filePaths = artifacts.Select(a => a.Uri.LocalPath).ToList();
                filePaths.Should().Contain(targetFile1);
                filePaths.Should().Contain(targetFile2);
                
                filePaths.Should().NotContain(path => path.Contains("realSymlink"));
                filePaths.Should().AllSatisfy(path => path.Should().Contain("realTarget"));
            }
            finally
            {
                CleanupDirectoryOrFile(new[] { testBaseDir });
            }
        }

        [Fact]
        public void OrderedFileSpecifier_HandlesSymbolicLinkFiles()
        {
            string tempFolder = Path.GetTempPath();
            string testBaseDir = Path.Combine(tempFolder, Guid.NewGuid().ToString());
            string targetFile = Path.Combine(testBaseDir, "realTargetFile.txt");
            string symlinkFile = Path.Combine(testBaseDir, "symlinkFile.txt");

            try
            {
                Directory.CreateDirectory(testBaseDir);
                File.WriteAllText(targetFile, "This is the real target file");

                bool createdSymlink = TryCreateSymbolicLink(symlinkFile, targetFile, isDirectory: false);
                createdSymlink.Should().BeTrue("symbolic link creation should succeed for this test to be valid");

                File.Exists(symlinkFile).Should().BeTrue();
                var fileSystem = new FileSystem();
                fileSystem.IsSymbolicLink(symlinkFile).Should().BeTrue();
                fileSystem.IsSymbolicLink(targetFile).Should().BeFalse();

                var specifier = new OrderedFileSpecifier(Path.Combine(testBaseDir, "*.txt"), recurse: false);
                var artifacts = specifier.Artifacts.ToList();

                artifacts.Count.Should().Be(2);
                
                var filePaths = artifacts.Select(a => a.Uri.LocalPath).ToList();
                filePaths.Should().Contain(targetFile);
                filePaths.Should().Contain(symlinkFile);
            }
            finally
            {
                CleanupDirectoryOrFile(new[] { testBaseDir });
            }
        }

        [Fact]
        public void OrderedFileSpecifier_PreventsInfiniteLoopsWithCircularSymlinks()
        {
            string tempFolder = Path.GetTempPath();
            string testBaseDir = Path.Combine(tempFolder, Guid.NewGuid().ToString());
            string subDir1 = Path.Combine(testBaseDir, "subdir1");
            string subDir2 = Path.Combine(testBaseDir, "subdir2");
            string symlinkToParent = Path.Combine(subDir1, "linkToParent");
            string symlinkToSibling = Path.Combine(subDir1, "linkToSibling");
            string testFile1 = Path.Combine(subDir1, "file1.txt");
            string testFile2 = Path.Combine(subDir2, "file2.txt");

            try
            {
                Directory.CreateDirectory(testBaseDir);
                Directory.CreateDirectory(subDir1);
                Directory.CreateDirectory(subDir2);
                File.WriteAllText(testFile1, "File in subdir1");
                File.WriteAllText(testFile2, "File in subdir2");

                bool createdParentLink = TryCreateSymbolicLink(symlinkToParent, testBaseDir, isDirectory: true);
                bool createdSiblingLink = TryCreateSymbolicLink(symlinkToSibling, subDir2, isDirectory: true);

                createdParentLink.Should().BeTrue("symbolic link to parent directory creation should succeed for this test to be valid");
                createdSiblingLink.Should().BeTrue("symbolic link to sibling directory creation should succeed for this test to be valid");

                Directory.Exists(symlinkToParent).Should().BeTrue();
                Directory.Exists(symlinkToSibling).Should().BeTrue();
                var fileSystem = new FileSystem();
                fileSystem.IsSymbolicLink(symlinkToParent).Should().BeTrue();
                fileSystem.IsSymbolicLink(symlinkToSibling).Should().BeTrue();

                var specifier = new OrderedFileSpecifier(Path.Combine(testBaseDir, "*.txt"), recurse: true);
                var artifacts = specifier.Artifacts.ToList();

                artifacts.Count.Should().Be(2);
                
                var filePaths = artifacts.Select(a => a.Uri.LocalPath).ToList();
                filePaths.Should().Contain(testFile1);
                filePaths.Should().Contain(testFile2);
                
                filePaths.Should().NotContain(path => path.Contains("linkToParent"));
                filePaths.Should().NotContain(path => path.Contains("linkToSibling"));
            }
            finally
            {
                CleanupDirectoryOrFile(new[] { testBaseDir });
            }
        }

        /// <summary>
        /// Attempts to create a symbolic link using cross-platform .NET methods.
        /// Returns true if successful, false if symbolic links are not supported or permissions are insufficient.
        /// </summary>
        private bool TryCreateSymbolicLink(string linkPath, string targetPath, bool isDirectory)
        {
            try
            {
                if (isDirectory)
                {
                    Directory.CreateSymbolicLink(linkPath, targetPath);
                }
                else
                {
                    File.CreateSymbolicLink(linkPath, targetPath);
                }
                return true;
            }
            catch
            {
                // If any exception occurs during symbolic link creation, return false
                // This handles cases where:
                // - Insufficient permissions (common on Windows without admin rights)
                // - Filesystem doesn't support symbolic links
                // - Other platform-specific issues
                return false;
            }
        }

        private void CleanupDirectoryOrFile(string[] paths)
        {
            foreach (string path in paths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    else if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch
                {
                    // Ignore cleanup errors to avoid test failures due to cleanup issues
                }
            }
        }
    }
}
