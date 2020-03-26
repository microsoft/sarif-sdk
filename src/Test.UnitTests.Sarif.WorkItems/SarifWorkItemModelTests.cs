// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItems;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Microsoft.WorkItems;
using System.Reflection;
using System.Resources;
using Xunit;


namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemModelTests
    {
        protected static readonly ResourceManager ResourceManager = new ResourceManager(typeof(WorkItemsResources));

        [Fact]
        public void SarifWorkItemModel_PopulatesDescription()
        {
            var context = new SarifWorkItemContext();
            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();
           
            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.BodyOrDescription.Should().NotBeNullOrEmpty();
            workItemModel.BodyOrDescription.Should().Contain(nameof(TestData.TestToolName));
            workItemModel.BodyOrDescription.Should().Contain(sarifLog.Runs[0].VersionControlProvenance[0].RepositoryUri.OriginalString);
            workItemModel.BodyOrDescription.Should().Contain(ResourceManager.GetString("GeneralFooterText"));
            workItemModel.BodyOrDescription.Should().NotContain(ResourceManager.GetString("AdoViewingOptions"));
        }

        [Fact]
        public void SarifWorkItemModel_PopulatesAdoDescription()
        {
            var context = new SarifWorkItemContext();
            context.CurrentProvider = FilingClient.SourceControlProvider.AzureDevOps;
            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();

            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.BodyOrDescription.Should().NotBeNullOrEmpty();
            workItemModel.BodyOrDescription.Should().Contain(nameof(TestData.TestToolName));
            workItemModel.BodyOrDescription.Should().Contain(sarifLog.Runs[0].VersionControlProvenance[0].RepositoryUri.OriginalString);
            workItemModel.BodyOrDescription.Should().Contain(ResourceManager.GetString("ViewScansTabResults"));
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
            workItemModel.BodyOrDescription.Should().Contain(ResourceManager.GetString("ViewScansTabResults"));
        }
    }
}
