// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Moq;
using System;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling
{
    public class WorkItemFilerTests
    {
        private static readonly ResourceExtractor extractor = new ResourceExtractor(typeof(WorkItemFilerTests));

        [Fact]
        public void WorkItemFile_RequiresAFileSystem()
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
    }
}
