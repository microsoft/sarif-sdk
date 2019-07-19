// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling
{
    public class WorkItemFilerTests
    {
        private static readonly ResourceExtractor s_extractor = new ResourceExtractor(typeof(WorkItemFilerTests));

        [Fact]
        public void WorkItemFiler_RequiresAFileSystem()
        {
            WorkItemFiler filer;
            Action action = () => filer = new WorkItemFiler(fileSystem: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_ChecksPathArgument()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            IFileSystem fileSystem = mockFileSystem.Object;
            var filer = new WorkItemFiler(fileSystem);

            Action action = () => filer.FileWorkItems(path: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_RejectsInvalidSarifFile()
        {
            const string LogFilePath = @"Invalid.sarif";
            string logFileContents = s_extractor.GetResourceText(LogFilePath);

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.ReadAllText(LogFilePath)).Returns(logFileContents);

            IFileSystem fileSystem = mockFileSystem.Object;
            var filer = new WorkItemFiler(fileSystem);

            Action action = () => filer.FileWorkItems(LogFilePath);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void WorkItemFiler_AcceptsSarifFileWithNullResults()
        {
            const string LogFilePath = @"NullResults.sarif";
            string logFileContents = s_extractor.GetResourceText(LogFilePath);

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.ReadAllText(LogFilePath)).Returns(logFileContents);

            IFileSystem fileSystem = mockFileSystem.Object;
            var filer = new WorkItemFiler(fileSystem);

            Action action = () => filer.FileWorkItems(LogFilePath);

            action.Should().NotThrow();
        }

        [Fact]
        public void WorkItemFiler_AcceptsSarifFileWithEmptyResults()
        {
            const string LogFilePath = @"EmptyResults.sarif";
            string logFileContents = s_extractor.GetResourceText(LogFilePath);

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.ReadAllText(LogFilePath)).Returns(logFileContents);

            IFileSystem fileSystem = mockFileSystem.Object;
            var filer = new WorkItemFiler(fileSystem);

            Action action = () => filer.FileWorkItems(LogFilePath);

            action.Should().NotThrow();
        }
    }
}
