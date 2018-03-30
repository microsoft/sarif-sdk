// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Moq;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class CodeAnalysisResultManagerTests
    {
        private IFileSystem fileSystem;

        // The list of files for which File.Exists should return true.
        private List<string> existingFiles;

        // The rebaselined path selected by the user.
        private string rebaselinedFileName;

        // The number of times we prompt the user for the resolved path.
        private int numPrompts;

        public CodeAnalysisResultManagerTests()
        {
            SarifViewerPackage.IsUnitTesting = true;

            this.existingFiles = new List<string>();

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem
                .Setup(fs => fs.FileExists(It.IsAny<string>()))
                .Returns((string path) => this.existingFiles.Contains(path));

            this.fileSystem = mockFileSystem.Object;
        }

        [Fact]
        public void CodeAnalysisResultManager_GetRebaselinedFileName_AcceptsMatchingFileNameFromUser()
        {
            // Arrange.
            const string FileNameInLogFile = @"C:\Code\sarif-sdk\src\Sarif\Notes.cs";
            const string RebaselinedFileName = @"D:\Users\John\source\sarif-sdk\src\Sarif\Notes.cs";

            this.rebaselinedFileName = RebaselinedFileName;

            var target = new CodeAnalysisResultManager(
                null,                               // This test never touches the file system.
                this.FakePromptForResolvedPath);

            // Act.
            string actualRebaselinedFileName = target.GetRebaselinedFileName(uriBaseId: null, pathFromLogFile: FileNameInLogFile);

            // Assert.
            actualRebaselinedFileName.Should().Be(RebaselinedFileName);

            Tuple<string, string>[] remappedPathPrefixes = target.GetRemappedPathPrefixes();
            remappedPathPrefixes.Length.Should().Be(1);
            remappedPathPrefixes[0].Item1.Should().Be(@"C:\Code");
            remappedPathPrefixes[0].Item2.Should().Be(@"D:\Users\John\source");
        }

        [Fact]
        public void CodeAnalysisResultManager_GetRebaselinedFileName_UsesExistingMapping()
        {
            // Arrange.
            const string FirstFileNameInLogFile = @"C:\Code\sarif-sdk\src\Sarif\Notes.cs";
            const string FirstRebaselinedFileName = @"D:\Users\John\source\sarif-sdk\src\Sarif\Notes.cs";

            const string SecondFileNameInLogFile = @"C:\Code\sarif-sdk\src\Sarif.UnitTests\JsonTests.cs";
            const string SecondRebaselinedFileName = @"D:\Users\John\source\sarif-sdk\src\Sarif.UnitTests\JsonTests.cs";

            this.existingFiles.Add(SecondRebaselinedFileName);

            this.rebaselinedFileName = FirstRebaselinedFileName;

            var target = new CodeAnalysisResultManager(
                this.fileSystem, 
                this.FakePromptForResolvedPath);

            // First, rebase a file to prime the list of mappings.
            target.GetRebaselinedFileName(uriBaseId: null, pathFromLogFile: FirstFileNameInLogFile);

            // The first time, we prompt the user for the name of the file to rebaseline to.
            this.numPrompts.Should().Be(1);

            // Act: Rebaseline a second file with the same prefix.
            string actualRebaselinedFileName = target.GetRebaselinedFileName(uriBaseId: null, pathFromLogFile: SecondFileNameInLogFile);

            // Assert.
            actualRebaselinedFileName.Should().Be(SecondRebaselinedFileName);

            Tuple<string, string>[] remappedPathPrefixes = target.GetRemappedPathPrefixes();
            remappedPathPrefixes.Length.Should().Be(1);
            remappedPathPrefixes[0].Item1.Should().Be(@"C:\Code");
            remappedPathPrefixes[0].Item2.Should().Be(@"D:\Users\John\source");

            // The second time, since the existing mapping suffices for the second file,
            // it's not necessary to prompt again.
            this.numPrompts.Should().Be(1);
        }

        [Fact]
        public void CodeAnalysisResultManager_GetRebaselinedFileName_IgnoresMismatchedFileNameFromUser()
        {
            // Arrange.
            const string FileNameInLogFile = @"C:\Code\sarif-sdk\src\Sarif\Notes.cs";
            const string RebaselinedFileName = @"D:\Users\John\source\sarif-sdk\src\Sarif\HashData.cs";

            this.rebaselinedFileName = RebaselinedFileName;

            var target = new CodeAnalysisResultManager(
                null,                               // This test never touches the file system.
                this.FakePromptForResolvedPath);

            // Act.
            string actualRebaselinedFileName = target.GetRebaselinedFileName(uriBaseId: null, pathFromLogFile: FileNameInLogFile);

            // Assert.
            actualRebaselinedFileName.Should().Be(FileNameInLogFile);

            Tuple<string, string>[] remappedPathPrefixes = target.GetRemappedPathPrefixes();
            remappedPathPrefixes.Should().BeEmpty();
        }

        [Fact]
        public void CodeAnalysisResultManager_GetRebaselinedFileName_WhenUserDoesNotSelectRebaselinedPath_UsesPathFromLogFile()
        {
            // Arrange.
            const string FileNameInLogFile = @"C:\Code\sarif-sdk\src\Sarif\Notes.cs";

            // The user does not select a file in the File Open dialog:
            this.rebaselinedFileName = null;

            var target = new CodeAnalysisResultManager(
                null,                               // This test never touches the file system.
                this.FakePromptForResolvedPath);

            // Act.
            string actualRebaselinedFileName = target.GetRebaselinedFileName(uriBaseId: null, pathFromLogFile: FileNameInLogFile);

            // Assert.
            actualRebaselinedFileName.Should().Be(FileNameInLogFile);

            Tuple<string, string>[] remappedPathPrefixes = target.GetRemappedPathPrefixes();
            remappedPathPrefixes.Should().BeEmpty();
        }

        [Fact]
        public void CodeAnalysisResultManager_GetRebaselinedFileName_WhenRebaselinedPathDiffersOnlyInDriveLetter_ReturnsRebaselinedPath()
        {
            // Arrange.
            const string FileNameInLogFile = @"C:\Code\sarif-sdk\src\Sarif\Notes.cs";
            const string RebaselinedFileName = @"D:\Code\sarif-sdk\src\Sarif\Notes.cs";

            this.rebaselinedFileName = RebaselinedFileName;

            var target = new CodeAnalysisResultManager(
                null,                               // This test never touches the file system.
                this.FakePromptForResolvedPath);

            // Act.
            string actualRebaselinedFileName = target.GetRebaselinedFileName(uriBaseId: null, pathFromLogFile: FileNameInLogFile);

            // Assert.
            actualRebaselinedFileName.Should().Be(RebaselinedFileName);

            Tuple<string, string>[] remappedPathPrefixes = target.GetRemappedPathPrefixes();
            remappedPathPrefixes.Length.Should().Be(1);
            remappedPathPrefixes[0].Item1.Should().Be(@"C:");
            remappedPathPrefixes[0].Item2.Should().Be(@"D:");
        }

        [Fact]
        public void CodeAnalysisResultManager_GetRebaselinedFileName_WhenRebaselinedPathHasMoreComponents_ReturnsRebaselinedPath()
        {
            // Arrange.
            const string FileNameInLogFile = @"C:\Code\sarif-sdk\src\Sarif\Notes.cs";
            const string RebaselinedFileName = @"C:\Users\Mary\Code\sarif-sdk\src\Sarif\Notes.cs";

            this.rebaselinedFileName = RebaselinedFileName;

            var target = new CodeAnalysisResultManager(
                null,                               // This test never touches the file system.
                this.FakePromptForResolvedPath);

            // Act.
            string actualRebaselinedFileName = target.GetRebaselinedFileName(uriBaseId: null, pathFromLogFile: FileNameInLogFile);

            // Assert.
            actualRebaselinedFileName.Should().Be(RebaselinedFileName);

            Tuple<string, string>[] remappedPathPrefixes = target.GetRemappedPathPrefixes();
            remappedPathPrefixes.Length.Should().Be(1);
            remappedPathPrefixes[0].Item1.Should().Be(@"C:");
            remappedPathPrefixes[0].Item2.Should().Be(@"C:\Users\Mary");
        }

        private string FakePromptForResolvedPath(string fullPathFromLogFile)
        {
            ++this.numPrompts;
            return this.rebaselinedFileName;
        }
    }
}
