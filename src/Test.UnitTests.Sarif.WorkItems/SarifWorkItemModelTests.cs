// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Microsoft.WorkItems;
using Xunit;


namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemModelTests
    {

        [Fact]
        public void SarifWorkItemModel_PopulatesGitHubDescription()
        {
            var context = new SarifWorkItemContext();
            context.CurrentProvider = FilingClient.SourceControlProvider.Github;

            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();

            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.BodyOrDescription.Should().NotBeNullOrEmpty();
            workItemModel.BodyOrDescription.Should().Contain(nameof(TestData.TestToolName));
            workItemModel.BodyOrDescription.Should().Contain(sarifLog.Runs[0].VersionControlProvenance[0].RepositoryUri.OriginalString);
            workItemModel.BodyOrDescription.Should().Contain(WorkItemsResources.GitHubDefaultDescriptionFooter);
            workItemModel.BodyOrDescription.Should().NotContain(WorkItemsResources.AzureDevOpsDefaultDescriptionFooter);
        }

        [Fact]
        public void SarifWorkItemModel_PopulatesAdoDescription()
        {
            // Context object specifies ADO as a filing client provider by default
            var context = new SarifWorkItemContext();
            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();

            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.BodyOrDescription.Should().NotBeNullOrEmpty();
            workItemModel.BodyOrDescription.Should().Contain(nameof(TestData.TestToolName));
            workItemModel.BodyOrDescription.Should().Contain(sarifLog.Runs[0].VersionControlProvenance[0].RepositoryUri.OriginalString);
            workItemModel.BodyOrDescription.Should().Contain(WorkItemsResources.AzureDevOpsDefaultDescriptionFooter);
            workItemModel.BodyOrDescription.Should().NotContain(WorkItemsResources.GitHubDefaultDescriptionFooter);
        }

        [Fact]
        public void SarifWorkItemModel_IncorporatesMultipleToolNamesIntoAdoDescription()
        {
            var context = new SarifWorkItemContext();
            context.CurrentProvider = FilingClient.SourceControlProvider.AzureDevOps;
            SarifLog sarifLog = TestData.CreateTwoRunThreeResultLog();

            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.BodyOrDescription.Should().NotBeNullOrEmpty();
            workItemModel.BodyOrDescription.Should().Contain(nameof(TestData.TestToolName));
            workItemModel.BodyOrDescription.Should().Contain(nameof(TestData.SecondTestToolName));
            workItemModel.BodyOrDescription.Should().Contain(TestData.FileLocations.Location1);
        }
    }
}
