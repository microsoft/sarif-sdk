// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.AzureDevOps;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling.AzureDevOps
{
    public class AzureDevOpsFilingTargetTests
    {
        [Fact]
        public void AzureDevopsFilingTarget_RequiresAzureDevOpsClient()
        {
            Action action = () => new AzureDevOpsFilingTarget(azureDevOpsClient: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AzureDevopsFilingTarget_CanBeCreated()
        {
            Action action = () => new AzureDevOpsFilingTarget(azureDevOpsClient: CreateMockAzureDevOpsClient());

            action.Should().NotThrow();
        }

        private static IAzureDevOpsClient CreateMockAzureDevOpsClient()
        {
            var mockAzureDevOpsClient = new Mock<IAzureDevOpsClient>();
            return mockAzureDevOpsClient.Object;
        }
    }
}