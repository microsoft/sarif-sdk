// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Grouping;
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
            Action action = () => filer = new WorkItemFiler(filingTarget: null, groupingStrategy: CreateMockGroupingStrategy(), fileSystem: CreateMockFileSystem());

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_RequiresAGroupingStrategy()
        {
            WorkItemFiler filer;
            Action action = () => filer = new WorkItemFiler(filingTarget: CreateMockFilingTarget(), groupingStrategy: null, fileSystem: CreateMockFileSystem());

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_RequiresAFileSystem()
        {
            WorkItemFiler filer;
            Action action = () => filer = new WorkItemFiler(filingTarget: CreateMockFilingTarget(), groupingStrategy: CreateMockGroupingStrategy(), fileSystem: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async void WorkItemFiler_ChecksPathArgument()
        {
            var filer = CreateWorkItemFiler();

            Func<Task> action = async () => await filer.FileWorkItems(logFilePath: null);

            await action.ShouldThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async void WorkItemFiler_RejectsInvalidSarifFile()
        {
            const string LogFilePath = "Invalid.sarif";
            WorkItemFiler filer = CreateWorkItemFiler(LogFilePath);

            Func<Task> action = async () => await filer.FileWorkItems(LogFilePath);

            await action.ShouldThrowAsync<ArgumentException>();
        }

        [Fact]
        public async void WorkItemFiler_AcceptsSarifFileWithNullResults()
        {
            const string LogFilePath = "NullResults.sarif";
            WorkItemFiler filer = CreateWorkItemFiler(LogFilePath);

            Func<Task> action = async () => await filer.FileWorkItems(LogFilePath);

            await action.ShouldNotThrowAsync();
        }

        [Fact]
        public async void WorkItemFiler_AcceptsSarifFileWithEmptyResults()
        {
            const string LogFilePath = "EmptyResults.sarif";
            WorkItemFiler filer = CreateWorkItemFiler(LogFilePath);

            Func<Task> action = async () => await filer.FileWorkItems(LogFilePath);

            await action.ShouldNotThrowAsync();
        }

        [Fact]
        public async Task WorkItemFiler_FilesWorkItemsOnlyForNewResults()
        {
            const string LogFilePath = "NewAndOldResults.sarif";
            WorkItemFiler filer = CreateWorkItemFiler(LogFilePath);

            IEnumerable<ResultGroup> filedWorkItems = await filer.FileWorkItems(LogFilePath);

            // The test file NewAndOldResults.sarif contains 5 results, but only 2 of them
            // have "baselineState": "new".
            filedWorkItems.Count().Should().Be(2);
        }

        [Fact]
        public async Task WorkItemFiler_FilesWorkItemsFromMultipleRuns()
        {
            const string LogFilePath = "MultipleRuns.sarif";
            WorkItemFiler filer = CreateWorkItemFiler(LogFilePath);

            IEnumerable<ResultGroup> filedWorkItems = await filer.FileWorkItems(LogFilePath);

            // The first run has 2 new results and the second run has 1.
            filedWorkItems.Count().Should().Be(3);
        }

        private static WorkItemFiler CreateWorkItemFiler(string logFilePath = null)
            => new WorkItemFiler(
                CreateMockFilingTarget(),
                CreateMockGroupingStrategy(),
                CreateMockFileSystem(logFilePath));

        private static FilingTarget CreateMockFilingTarget()
        {
            var mockFilingTarget = new Mock<FilingTarget>();

            // Moq magic: you can return whatever was passed to a method by providing
            // a lambda (rather than a fixed value) to Returns or ReturnsAsync.
            // https://stackoverflow.com/questions/996602/returning-value-that-was-passed-into-a-method
            mockFilingTarget
                .Setup(x => x.FileWorkItems(It.IsAny<IEnumerable<ResultGroup>>()))
                .ReturnsAsync((IEnumerable<ResultGroup> resultGroups) => resultGroups);

            return mockFilingTarget.Object;
        }

        // We aren't really creating a mock here; the "one result per work item"
        // strategy is simple enough to use reliably in unit tests.
        private static IGroupingStrategy CreateMockGroupingStrategy()
            => new OneResultPerWorkItemGroupingStrategy();

        private static IFileSystem CreateMockFileSystem(string logFilePath = null)
        {
            var mockFileSystem = new Mock<IFileSystem>();

            if (logFilePath != null)
            {
                string logFileContents = s_extractor.GetResourceText(logFilePath);

                mockFileSystem.Setup(x => x.ReadAllText(logFilePath)).Returns(logFileContents);
            }

            return mockFileSystem.Object;
        }
    }
}