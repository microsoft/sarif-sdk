// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

using Moq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Trait(TestTraits.WindowsOnly, "true")]
    public class GitHelperTests
    {
        [Fact]
        public void GetRepositoryRoot_WhenDotGitIsAbsent_ReturnsNull()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(false);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk\src")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk\src\Sarif")).Returns(true);

            var gitHelper = new GitHelper(mockFileSystem.Object);

            gitHelper.GetRepositoryRoot(@"C:\dev\sarif-sdk\src\Sarif").Should().BeNull();
        }

        [Fact]
        public void GetRepositoryRoot_WhenDotGitIsPresent_ReturnsTheDirectortyContainingDotGit()
        {
            string productDirectory = Path.GetDirectoryName(DirectoryHelpers.GetEnlistmentSrcDirectory());
            string repoDirectory = Directory.GetParent(productDirectory).FullName;

            var gitHelper = new GitHelper();

            gitHelper.GetRepositoryRoot(productDirectory).Should().BeEquivalentTo(repoDirectory);
        }

        [Fact]
        public void GetRepositoryRoot_ByDefault_PopulatesTheDirectoryToRepoRootCache()
        {
            // Returns '\src\' location within repo, e.g., 'c:\src\sarif-sdk\src'
            string productDirectory = Path.GetDirectoryName(DirectoryHelpers.GetEnlistmentSrcDirectory());
            string repoDirectory = Directory.GetParent(productDirectory).FullName;

            var gitHelper = new GitHelper();

            gitHelper.GetRepositoryRoot(repoDirectory).Should().BeEquivalentTo(repoDirectory);
            gitHelper.GetRepositoryRoot(productDirectory).Should().BeEquivalentTo(repoDirectory);

            // Verify that the API returns the correct results whether or not the cache is in use.
            gitHelper.GetRepositoryRoot(repoDirectory).Should().BeEquivalentTo(repoDirectory);
            gitHelper.GetRepositoryRoot(productDirectory).Should().BeEquivalentTo(repoDirectory);

            gitHelper.directoryToRepoRootPathDictionary.Count.Should().Be(2);
            gitHelper.directoryToRepoRootPathDictionary[repoDirectory].Should().BeEquivalentTo(repoDirectory);
            gitHelper.directoryToRepoRootPathDictionary[productDirectory].Should().BeEquivalentTo(repoDirectory);
        }

        [Fact]
        public void GetRepositoryRoot_WhenCachingIsDisabled_DoesNotPopulateTheDirectoryToRepoRootCache()
        {
            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(false);

            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk\.git")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk\src")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk\src\Sarif")).Returns(true);

            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\docs")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\docs\public")).Returns(true);

            var gitHelper = new GitHelper(mockFileSystem.Object);

            // Verify that the API returns the correct results whether or not the cache is in use.
            string topLevelDirectoryRoot = gitHelper.GetRepositoryRoot(@"C:\dev\sarif-sdk", useCache: false);
            string topLevelDirectoryRootAgain = gitHelper.GetRepositoryRoot(@"C:\dev\sarif-sdk", useCache: false);
            string subdirectoryRoot = gitHelper.GetRepositoryRoot(@"C:\dev\sarif-sdk\src\Sarif", useCache: false);
            string nonSourceControlledRoot = gitHelper.GetRepositoryRoot(@"C:\docs\public", useCache: false);

            gitHelper.directoryToRepoRootPathDictionary.Count.Should().Be(0);
        }

        [Fact]
        public void GetRepositoryRoot_WhenCalledOnTheDefaultInstanceWithNoParameters_Throws()
        {
            Action action = () => GitHelper.Default.GetRepositoryRoot(@"C:\dev");

            action.Should().Throw<ArgumentException>().WithMessage($"{SdkResources.GitHelperDefaultInstanceDoesNotPermitCaching}*");
        }

        [Fact]
        public void GetRepositoryRoot_WhenCalledOnTheDefaultInstanceWithCachingDisabled_DoesNotThrow()
        {
            // If you have a directory with this name on your machine and it does not contain a .git
            // subdirectory, then the underlying invocation of `git rev-parse --show-toplevel` will
            // fail with the error "fatal: not a git repository (or any of the parent directories): .git",
            // the process will exit with exit code 128, and the ProcessRunner will throw an
            // InvalidOperationException. That will cause this test to fail, because it expects no
            // exceptions.
            // If you do _not_ have such a directory, GetRepositoryRoot will simply return null, and
            // the test will pass.
            const string NonexistentDirectory = @"C:\PleaseDoNotCreateADirectoryWithThisName";

            Action action = () => GitHelper.Default.GetRepositoryRoot(NonexistentDirectory, useCache: false);

            action.Should().NotThrow();
        }

        [Fact]
        public void GetGitExePath_WhenPathExistsInProgramFiles()
        {
            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            var gitHelper = new GitHelper(mockFileSystem.Object);

            gitHelper.GitExePath.Should().NotBeNullOrEmpty();
            GitHelper.GetGitExePath(mockFileSystem.Object).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GetGitExePath_WhenPathDoesNotExistInProgramFiles()
        {
            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var gitHelper = new GitHelper(mockFileSystem.Object);

            gitHelper.GitExePath.Should().BeNull();
            GitHelper.GetGitExePath(mockFileSystem.Object).Should().BeNull();
        }

        [Fact]
        public void SearchForFileInEnvironmentVariable_WhenVariableDoesNotExist()
        {
            FileSearcherHelper.SearchForFileInEnvironmentVariable("PATH_THAT_DOES_NOT_EXIST", "filename.exe").Should().BeNull();
        }

        [Fact]
        public void SearchForFileInEnvironmentVariable_WhenVariableExistsButFileDoesNot()
        {
            // The error in the ntdll name here is intentional.
            FileSearcherHelper.SearchForFileInEnvironmentVariable("PATH", "ntdll.dlll").Should().BeNull();
        }

        [Fact]
        public void SearchForFileInEnvironmentVariable_WhenVariableAndFileExists()
        {
            FileSearcherHelper.SearchForFileInEnvironmentVariable("PATH", "ntdll.dll").Should().NotBeNull();
        }

        [Fact]
        public void GitExePath_WhenPathDoesNotExist_SettingManuallyShouldWork()
        {
            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var gitHelper = new GitHelper(mockFileSystem.Object);

            gitHelper.GitExePath = @"C:\dev";
            gitHelper.GitExePath.Should().Be(@"C:\dev");
        }

        [Fact]
        public void GetTopLevel_WhenRepoPathIsToAFile()
        {
            string pathToFile = Path.Combine(Environment.CurrentDirectory, "UnusedFileName.txt");
            string repoRootPath = GitHelper.Default.GetTopLevel(pathToFile);
            repoRootPath.Should().NotBeNull();
            pathToFile.StartsWith(repoRootPath, StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();
        }

        [Fact]
        public void GetTopLevel_WhenRepoPathIsToADirectory()
        {
            string pathToDirectory = Environment.CurrentDirectory;
            string repoRootPath = GitHelper.Default.GetTopLevel(pathToDirectory);
            repoRootPath.Should().NotBeNull();
            pathToDirectory.StartsWith(repoRootPath, StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();
        }

        [Fact]
        public void GetTopLevel_WhenRepoPathDoesNotExist()
        {
            string path = @"X:\DoesnotExistDirectory";
            GitHelper.Default.GetTopLevel(path).Should().BeNull();
        }

        [Fact]
        public void GetTopLevel_WhenRepoPathHasInvalidChars()
        {
            string path = @"C:\Invalid:\fo/lder\sub?folder";
            GitHelper.Default.GetTopLevel(path).Should().BeNull();
        }

        [Fact]
        public void GetTopLevel_WhenRepoPathIsNull()
        {
            string path = null;
            GitHelper.Default.GetTopLevel(path).Should().BeNull();
        }

        [Fact]
        public void GetTopLevel_WhenGitDoesnotExist()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(false);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk\src\Sarif")).Returns(true);

            var gitHelper = new GitHelper(mockFileSystem.Object);

            gitHelper.GetTopLevel(@"C:\dev\sarif-sdk\src\Sarif").Should().BeNull();
        }

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void GitHelper_GetBlame()
        {
            var gitHelper = new GitHelper();
            string repoDirectory = gitHelper.GetRepositoryRoot(this.GetType().Assembly.Location);
            string readMePath = Path.Combine(repoDirectory, "README.md");
            string blame = gitHelper.GetBlame(readMePath);

            // The original commit for our repo readme file.
            blame.Contains("3e9d5d8d9c8d00dfe534e9fa3108d594d54bfcb6").Should().BeTrue();
        }
    }
}
