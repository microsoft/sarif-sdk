// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Moq;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class CodeAnalysisResultManagerTests
    {
        private IKeyboard keyboard;
        private IFileSystem fileSystem;
        private ICommonDialogFactory commonDialogFactory;
        private Mock<IOpenFileDialog> mockOpenFileDialog;

        // The list of files for which File.Exists should return true.
        private List<string> existingFiles;

        // The value that OpenFileDialog.Result should return.
        private bool? openFileDialogResult;

        // The value that OpenFileDialog.FileName (the path to the file selected by the user)
        // should return.
        private string openFileDialogFileName;

        public CodeAnalysisResultManagerTests()
        {
            this.existingFiles = new List<string>();

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem
                .Setup(fs => fs.FileExists(It.IsAny<string>()))
                .Returns((string path) => this.existingFiles.Contains(path));

            this.fileSystem = mockFileSystem.Object;

            var mockKeyboard = new Mock<IKeyboard>();
            mockKeyboard.SetupGet(kb => kb.FocusedElement).Returns(default(IInputElement));

            this.keyboard = mockKeyboard.Object;

            this.mockOpenFileDialog = new Mock<IOpenFileDialog>();
            this.mockOpenFileDialog
                .SetupGet(ofd => ofd.FileName)
                .Returns(() => this.openFileDialogFileName);
            this.mockOpenFileDialog
                .Setup(ofd => ofd.ShowDialog())
                .Returns(() => this.openFileDialogResult);

            IOpenFileDialog openFileDialog = this.mockOpenFileDialog.Object;

            var mockCommonDialogFactory = new Mock<ICommonDialogFactory>();
            mockCommonDialogFactory
                .Setup(cdf => cdf.CreateOpenFileDialog())
                .Returns(() => openFileDialog);

            this.commonDialogFactory = mockCommonDialogFactory.Object;
        }

        [Fact]
        public void CodeAnalysisResultManager_GetRebaselinedFileName_AcceptsMatchingFileNameFromUser()
        {
            // Arrange.
            const string FileNameInLogFile = @"C:\Code\sarif-sdk\src\Sarif\Notes.cs";
            const string RebaselinedFileName = @"D:\Users\John\source\sarif-sdk\src\Sarif\Notes.cs";

            this.openFileDialogFileName = RebaselinedFileName;
            this.openFileDialogResult = true;

            var target = new CodeAnalysisResultManager(
                null,                               // This test never touches the file system.
                this.keyboard,
                this.commonDialogFactory);

            // Act.
            string rebaselinedFileName = target.GetRebaselinedFileName(uriBaseId: null, fileName: FileNameInLogFile);

            // Assert.
            rebaselinedFileName.Should().Be(RebaselinedFileName);

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

            this.openFileDialogFileName = FirstRebaselinedFileName;
            this.openFileDialogResult = true;

            var target = new CodeAnalysisResultManager(
                this.fileSystem, 
                this.keyboard,
                this.commonDialogFactory);

            // First, rebase a file to prime the list of mappings.
            target.GetRebaselinedFileName(uriBaseId: null, fileName: FirstFileNameInLogFile);

            // The first time, we prompt the user for the name of the file to rebaseline to.
            this.mockOpenFileDialog.Verify(ofd => ofd.ShowDialog(), Times.Once());

            // Act: Rebaseline a second file with the same prefix.
            string rebaselinedFileName = target.GetRebaselinedFileName(uriBaseId: null, fileName: SecondFileNameInLogFile);

            // Assert.
            rebaselinedFileName.Should().Be(SecondRebaselinedFileName);

            Tuple<string, string>[] remappedPathPrefixes = target.GetRemappedPathPrefixes();
            remappedPathPrefixes.Length.Should().Be(1);
            remappedPathPrefixes[0].Item1.Should().Be(@"C:\Code");
            remappedPathPrefixes[0].Item2.Should().Be(@"D:\Users\John\source");

            // The second time, since the existing mapping suffices for the second file,
            // it's not necessary to prompt again.
            this.mockOpenFileDialog.Verify(ofd => ofd.ShowDialog(), Times.Once());
        }

        [Fact]
        public void CodeAnalysisResultManager_GetRebaselinedFileName_IgnoresMismatchedFileNameFromUser()
        {
            // Arrange.
            const string FileNameInLogFile = @"C:\Code\sarif-sdk\src\Sarif\Notes.cs";
            const string RebaselinedFileName = @"D:\Users\John\source\sarif-sdk\src\Sarif\HashData.cs";

            this.openFileDialogFileName = RebaselinedFileName;
            this.openFileDialogResult = true;

            var target = new CodeAnalysisResultManager(
                null,                               // This test never touches the file system.
                this.keyboard,
                this.commonDialogFactory);

            // Act.
            string rebaselinedFileName = target.GetRebaselinedFileName(uriBaseId: null, fileName: FileNameInLogFile);

            // Assert.
            rebaselinedFileName.Should().Be(FileNameInLogFile);

            Tuple<string, string>[] remappedPathPrefixes = target.GetRemappedPathPrefixes();
            remappedPathPrefixes.Should().BeEmpty();
        }
    }
}
