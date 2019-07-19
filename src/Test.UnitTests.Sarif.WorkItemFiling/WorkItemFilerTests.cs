// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling
{
    public class WorkItemFiler
    {
        [Fact]
        public void WorkItemFiler_Exists()
        {
            var filer = new WorkItemFiler();
            filer.Should().NotBeNull();
        }
    }
}
