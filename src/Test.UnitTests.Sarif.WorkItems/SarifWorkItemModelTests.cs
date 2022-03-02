// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

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

        [Fact]
        public void SarifWorkItemModel_WithAdditonalTags()
        {
            string tag1 = "E2E_test_run";
            string tag2 = "PPE";
            var context = new SarifWorkItemContext();
            context.CurrentProvider = FilingClient.SourceControlProvider.Github;
            context.AdditionalTags = new StringSet(new[] { tag1, tag2 });

            SarifLog sarifLog = TestData.CreateSimpleLog();

            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.Should().NotBeNull();
            workItemModel.LabelsOrTags.Should().NotBeNullOrEmpty();
            workItemModel.LabelsOrTags.Count.Should().Be(2);
            workItemModel.LabelsOrTags.Contains(tag1).Should().BeTrue();
            workItemModel.LabelsOrTags.Contains(tag2).Should().BeTrue();
        }

        [Fact]
        public void SarifWorkItemModel_WithWorkItemUri()
        {
            var context = new SarifWorkItemContext();
            context.CurrentProvider = FilingClient.SourceControlProvider.Github;

            var workItemUri = new Uri("https://dev.azure.com/org/project/workitem/12345");
            SarifLog sarifLog = TestData.CreateSimpleLog();
            sarifLog.Runs[0].Results[0].WorkItemUris = new[] { workItemUri };

            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.Should().NotBeNull();
            workItemModel.Uri.OriginalString.Should().Be(workItemUri.OriginalString);
        }

        [Fact]
        public void SarifWorkItemModel_NullSarifLog()
        {
            var context = new SarifWorkItemContext();
            context.CurrentProvider = FilingClient.SourceControlProvider.AzureDevOps;

            Assert.Throws<ArgumentNullException>(() => new SarifWorkItemModel(sarifLog: null, context));
        }
    }
}
