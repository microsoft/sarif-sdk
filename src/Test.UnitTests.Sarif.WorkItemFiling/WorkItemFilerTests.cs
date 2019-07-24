// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
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
            Action action = () => filer = new WorkItemFiler(filingTarget: null, filteringStrategy: CreateFilteringStrategy(), groupingStrategy: CreateGroupingStrategy(), fileSystem: CreateMockFileSystem());

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_RequiresAFilteringStrategy()
        {
            WorkItemFiler filer;
            Action action = () => filer = new WorkItemFiler(filingTarget: CreateMockFilingTarget(), filteringStrategy: null, groupingStrategy: CreateGroupingStrategy(), fileSystem: CreateMockFileSystem());

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_RequiresAGroupingStrategy()
        {
            WorkItemFiler filer;
            Action action = () => filer = new WorkItemFiler(filingTarget: CreateMockFilingTarget(), filteringStrategy: CreateFilteringStrategy(), groupingStrategy: null, fileSystem: CreateMockFileSystem());

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_UsesARealFileSystemByDefault()
        {
            WorkItemFiler filer = new WorkItemFiler(filingTarget: CreateMockFilingTarget(), filteringStrategy: CreateFilteringStrategy(), groupingStrategy: CreateGroupingStrategy(), fileSystem: null);

            filer.FileSystem.GetType().FullName.Should().Be(typeof(FileSystem).FullName);
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

            // The test file NewAndOldResults.sarif contains 5 results, only 2 of them
            // have "baselineState": "new", but the "all results" filter will take all of them.
            filedWorkItems.Count().Should().Be(5);
        }

        [Fact]
        public async Task WorkItemFiler_FilesWorkItemsFromMultipleRuns()
        {
            const string LogFilePath = "MultipleRuns.sarif";
            WorkItemFiler filer = CreateWorkItemFiler(LogFilePath);

            // Compute the number of work items we expect to file, using the trivial
            // "all results" filtering strategy and the "one result per work item"
            // grouping strategy that these unit tests employ.
            SarifLog sarifLog = Microsoft.CodeAnalysis.Sarif.Utilities.GetSarifLogFromResource(s_extractor, LogFilePath);
            int expectedResultsCount = sarifLog.Runs.SelectMany(run => run.Results).Count();

            IEnumerable<ResultGroup> filedWorkItems = await filer.FileWorkItems(LogFilePath);

            filedWorkItems.Count().Should().Be(expectedResultsCount);
        }

        [Fact]
        public async Task WorkItemFiler_ThrowsWhenItsFilteringStrategyThrows()
        {
            const string LogFilePath = "MultipleRuns.sarif";

            var mockFilteringStrategy = new Mock<FilteringStrategy>();
            mockFilteringStrategy.Setup(x => x.FilterResults(It.IsAny<IList<Result>>())).Throws(new TestException());

            WorkItemFiler filer = new WorkItemFiler(
                CreateMockFilingTarget(),
                mockFilteringStrategy.Object,
                CreateGroupingStrategy(),
                CreateMockFileSystem(LogFilePath));

            Func<Task> action = async () => await filer.FileWorkItems(LogFilePath);

            await action.ShouldThrowAsync<TestException>();
        }

        [Fact]
        public async Task WorkItemFiler_ThrowsWhenItsGroupingStrategyThrows()
        {
            const string LogFilePath = "MultipleRuns.sarif";

            var mockGroupingStrategy = new Mock<GroupingStrategy>();
            mockGroupingStrategy.Setup(x => x.GroupResults(It.IsAny<IList<Result>>())).Throws(new TestException());

            WorkItemFiler filer = new WorkItemFiler(
                CreateMockFilingTarget(),
                CreateFilteringStrategy(),
                mockGroupingStrategy.Object,
                CreateMockFileSystem(LogFilePath));

            Func<Task> action = async () => await filer.FileWorkItems(LogFilePath);

            await action.ShouldThrowAsync<TestException>();
        }

        private static WorkItemFiler CreateWorkItemFiler(string logFilePath = null)
            => new WorkItemFiler(
                CreateMockFilingTarget(),
                CreateFilteringStrategy(),
                CreateGroupingStrategy(),
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

        // The "one result per work item" grouping strategy is simple enough to use reliably
        // in unit tests without creating a mock.
        private static GroupingStrategy CreateGroupingStrategy()
            => new OneResultPerWorkItemGroupingStrategy();

        // The "all results" filtering strategy is simple enough to use reliably in unit tests
        // without creating a mock.
        private static FilteringStrategy CreateFilteringStrategy()
            => new AllResultsFilteringStrategy();

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