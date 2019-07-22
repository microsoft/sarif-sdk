// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.AzureDevOps;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling.AzureDevOps
{
    public class AzureDevOpsFilingTargetTests
    {
        [Fact]
        public void AzureDevopsFilingTarget_Exists()
        {
            var filingTarget = new AzureDevOpsFilingTarget();
            filingTarget.Should().NotBeNull();
        }
    }
}