// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItems;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Xunit;


namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItems
{
    public class SarifWorkItemModelTests
    {
        [Fact]
        public void SarifWorkItemModel_PopulatesDescription()
        {
            var context = new SarifWorkItemContext();
            SarifLog sarifLog = TestData.SarifLogs.OneIdThreeLocations;

            var workItemModel = new SarifWorkItemModel(sarifLog, context);
            workItemModel.BodyOrDescription.Should().NotBeNullOrEmpty();
            workItemModel.BodyOrDescription.Should().Contain(nameof(TestData.TestToolName));
        }
    }
}
