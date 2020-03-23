// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItems;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Microsoft.WorkItems;
using Xunit;


namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItems
{
    public class SarifWorkItemModelTests
    {
        [Fact]
        public void SarifWorkItemModel_PopulatesDescription()
        {
            var context = new SarifWorkItemContext();
            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();

            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.BodyOrDescription.Should().NotBeNullOrEmpty();
            workItemModel.BodyOrDescription.Should().Contain(nameof(TestData.TestToolName));
            workItemModel.BodyOrDescription.Should().Contain("Details for the above issues can be found in the attachment filed with this issue.");
            workItemModel.BodyOrDescription.Should().NotContain("Scans tab");
        }

        [Fact]
        public void SarifWorkItemModel_PopulatesADODescription()
        {
            var context = new SarifWorkItemContext();
            context.CurrentProvider = FilingClient.SourceControlProvider.AzureDevOps;
            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();

            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.BodyOrDescription.Should().NotBeNullOrEmpty();
            workItemModel.BodyOrDescription.Should().Contain(nameof(TestData.TestToolName));
            workItemModel.BodyOrDescription.Should().Contain("To see result details, please visit the Scans tab of this bug, or the attached SARIF log.");
        }
    }
}
