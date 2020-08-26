// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class GitInformationTests
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

            var gitInformation = new GitInformation(mockFileSystem.Object);

            gitInformation.GetRepositoryRoot(@"C:\dev\sarif-sdk\src\Sarif").Should().BeNull();
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

            var gitInformation = new GitInformation(mockFileSystem.Object);

            gitInformation.GetRepositoryRoot(@"C:\dev\sarif-sdk\src\Sarif").Should().Be(@"C:\dev\sarif-sdk");
        }
    }
}
