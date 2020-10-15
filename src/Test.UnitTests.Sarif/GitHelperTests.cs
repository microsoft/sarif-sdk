// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Moq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
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
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(false);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk\.git")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk\src")).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(@"C:\dev\sarif-sdk\src\Sarif")).Returns(true);

            var gitHelper = new GitHelper(mockFileSystem.Object);

            gitHelper.GetRepositoryRoot(@"C:\dev\sarif-sdk\src\Sarif").Should().Be(@"C:\dev\sarif-sdk\");
        }

        [Fact]
        public void GetRepositoryRoot_ByDefault_PopulatesTheDirectoryToRepoRootCache()
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

            string topLevelDirectoryRoot = gitHelper.GetRepositoryRoot(@"C:\dev\sarif-sdk");
            string topLevelDirectoryRootAgain = gitHelper.GetRepositoryRoot(@"C:\dev\sarif-sdk");
            string subdirectoryRoot = gitHelper.GetRepositoryRoot(@"C:\dev\sarif-sdk\src\Sarif");
            string nonSourceControlledRoot = gitHelper.GetRepositoryRoot(@"C:\docs\public");

            // Verify that the API returns the correct results whether or not the cache is in use.
            topLevelDirectoryRoot.Should().Be(@"C:\dev\sarif-sdk\");
            topLevelDirectoryRootAgain.Should().Be(topLevelDirectoryRoot);
            subdirectoryRoot.Should().Be(topLevelDirectoryRoot);
            nonSourceControlledRoot.Should().BeNull();

            gitHelper.directoryToRepoRootPathDictionary.Count.Should().Be(3);
            gitHelper.directoryToRepoRootPathDictionary[@"C:\dev\sarif-sdk\src\Sarif"].Should().Be(@"C:\dev\sarif-sdk\");
            gitHelper.directoryToRepoRootPathDictionary[@"C:\dev\sarif-sdk"].Should().Be(@"C:\dev\sarif-sdk\");
            gitHelper.directoryToRepoRootPathDictionary[@"C:\docs\public"].Should().BeNull();
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
            Action action = () => GitHelper.Default.GetRepositoryRoot(@"C:\dev", useCache: false);

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

            gitHelper.GitExePath.Should().BeNull();

            gitHelper.GitExePath = @"C:\dev";
            gitHelper.GitExePath.Should().NotBeNullOrEmpty();
        }
    }
}
