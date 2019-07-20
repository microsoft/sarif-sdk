// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public void WorkItemFiler_RequiresAFilingTarget()
        {
            WorkItemFiler filer;
            Action action = () => filer = new WorkItemFiler(filingTarget: null, fileSystem: new FileSystem());

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_RequiresAFileSystem()
        {
            WorkItemFiler filer;
            Action action = () => filer = new WorkItemFiler(filingTarget: new TestFilingTarget(), fileSystem: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async void WorkItemFiler_ChecksPathArgument()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            IFileSystem fileSystem = mockFileSystem.Object;
            var filer = new WorkItemFiler(new TestFilingTarget(), fileSystem);

            Func<Task> action = async () => await filer.FileWorkItems(logFilePath: null);

            await action.ShouldThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async void WorkItemFiler_RejectsInvalidSarifFile()
        {
            const string LogFilePath = "Invalid.sarif";
            WorkItemFiler filer = CreateWorkItemFilerForResource(LogFilePath);

            Func<Task> action = async () => await filer.FileWorkItems(logFilePath: null);
            action = async () => await filer.FileWorkItems(LogFilePath);

            await action.ShouldThrowAsync<ArgumentException>();
        }

        [Fact]
        public async void WorkItemFiler_AcceptsSarifFileWithNullResults()
        {
            const string LogFilePath = "NullResults.sarif";
            WorkItemFiler filer = CreateWorkItemFilerForResource(LogFilePath);

            Func<Task> action = async () => await filer.FileWorkItems(LogFilePath);

            await action.ShouldNotThrowAsync();
        }

        [Fact]
        public async void WorkItemFiler_AcceptsSarifFileWithEmptyResults()
        {
            const string LogFilePath = "EmptyResults.sarif";
            WorkItemFiler filer = CreateWorkItemFilerForResource(LogFilePath);

            Func<Task> action = async () => await filer.FileWorkItems(LogFilePath);

            await action.ShouldNotThrowAsync();
        }

        [Fact]
        public async Task WorkItemFiler_FilesWorkItemsOnlyForNewResults()
        {
            const string LogFilePath = "NewAndOldResults.sarif";
            WorkItemFiler filer = CreateWorkItemFilerForResource(LogFilePath);

            IEnumerable<Result> filedResults = await filer.FileWorkItems(LogFilePath);

            // The test file NewAndOldResults.sarif contains 5 results, but only 2 of them
            // have "baselineState": "new".
            filedResults.Count().Should().Be(2);
        }

        private static WorkItemFiler CreateWorkItemFilerForResource(string resourceName)
        {
            string logFileContents = s_extractor.GetResourceText(resourceName);

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.ReadAllText(resourceName)).Returns(logFileContents);

            IFileSystem fileSystem = mockFileSystem.Object;

            WorkItemFilingTargetBase filingTarget = new TestFilingTarget();

            return new WorkItemFiler(filingTarget, fileSystem);
        }
    }
}
