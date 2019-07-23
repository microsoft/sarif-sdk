// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.AzureDevOps;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Grouping;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling.AzureDevOps
{
    public class AzureDevOpsFilingTargetTests
    {
        [Fact]
        public void AzureDevOpsFilingTarget_RequiresAzureDevOpsClient()
        {
            Action action = () => new AzureDevOpsFilingTarget(azureDevOpsClient: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AzureDevOpsFilingTarget_CanBeCreated()
        {
            Action action = () => new AzureDevOpsFilingTarget(azureDevOpsClient: CreateMockAzureDevOpsClient());

            action.Should().NotThrow();
        }

        [Fact]
        public async Task AzureDevOpsFilingTarget_IsNotYetImplemented()
        {
            var filingTarget = new AzureDevOpsFilingTarget(azureDevOpsClient: CreateMockAzureDevOpsClient());

            Func<Task> action = async () => await filingTarget.FileWorkItems(new List<ResultGroup>());

            await action.ShouldThrowAsync<NotImplementedException>();
        }

        private static IAzureDevOpsClient CreateMockAzureDevOpsClient()
        {
            var mockAzureDevOpsClient = new Mock<IAzureDevOpsClient>();
            return mockAzureDevOpsClient.Object;
        }
    }
}