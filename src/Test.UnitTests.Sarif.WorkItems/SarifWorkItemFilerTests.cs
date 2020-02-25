// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.WorkItems;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemFilerTests
    {
        private static readonly Uri s_testUri = new Uri("https://github.com/Microsoft/sarif-sdk");

        [Fact]
        public void WorkItemFiler_ValidatesSarifLogFileContentsArgument()
        {
            SarifWorkItemFiler filer = CreateWorkItemFiler();

            Action action = () => filer.FileWorkItems(sarifLogFileContents: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_ValidatesSarifLogFileArgument()
        {
            SarifWorkItemFiler filer = CreateWorkItemFiler();

            Action action = () => filer.FileWorkItems(sarifLog: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_ValidatesSarifLogFileLocationArgument()
        {
            SarifWorkItemFiler filer = CreateWorkItemFiler();

            Action action = () => filer.FileWorkItems(sarifLogFileLocation: null);

            action.Should().Throw<ArgumentNullException>();
        }

        private static SarifWorkItemFiler CreateWorkItemFiler()
            => CreateMockSarifWorkItemFiler();

        private static SarifWorkItemFiler CreateMockSarifWorkItemFiler()
        {
            var mockFiler = new Mock<SarifWorkItemFiler>();

            mockFiler
                .Setup(x => x.FileWorkItems(It.IsAny<string>()))
                .CallBase(); 

            mockFiler
                .Setup(x => x.FileWorkItems(It.IsAny<SarifLog>()))
                .CallBase();

            mockFiler
                .Setup(x => x.FileWorkItems(It.IsAny<Uri>()))
                .CallBase();
            
            // Moq magic: you can return whatever was passed to a method by providing
            // a lambda (rather than a fixed value) to Returns or ReturnsAsync.
            // https://stackoverflow.com/questions/996602/returning-value-that-was-passed-into-a-method
            mockFiler
                .Setup(x => x.FileWorkItems(
                    It.IsAny<Uri>(), 
                    It.IsAny<IList<WorkItemModel<SarifWorkItemContext>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync((Uri uri, IList<WorkItemModel<SarifWorkItemContext>> resultGroups, string personalAccessToken) => resultGroups);

            return mockFiler.Object;
        }

        private class TestFilingClient : FilingClient
        {
            public override Task Connect(string personalAccessToken = null)
            {
                throw new NotImplementedException();
            }

            public override Task<IEnumerable<WorkItemModel>> FileWorkItems(IEnumerable<WorkItemModel> workItemModels)
            {
                throw new NotImplementedException();
            }
        }
    }
}