// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemFilingContextTests
    {
        [Fact]
        public void WorkItemFilingContext_RoundTripsWorkItemModelTransformer()
        {
            var munger = new Munger();

            var context = new SarifWorkItemContext();

            context.AddWorkItemModelTransformer(munger);
            context.Transformers[0].GetType().Should().Be(munger.GetType());

            context = RoundTripThroughXml(context);

            context.Transformers[0].GetType().Should().Be(munger.GetType());

            string newAreaPath = Guid.NewGuid().ToString();
            context.SetProperty(Munger.NewAreaPath, newAreaPath);

            var workItemModel = new SarifWorkItemModel(sarifLog: TestData.CreateSimpleLog(), context);

            context.Transformers[0].Transform(workItemModel);
            workItemModel.Area.Should().Be(newAreaPath);
        }

        [Fact]
        public void WorkItemFilingContext_NullSarifLogRaisesArgumentNullException()
        {
            var areaPathTransformer = new AreaPathFromUri();

            var context = new SarifWorkItemContext();

            context.AddWorkItemModelTransformer(areaPathTransformer);
            context.Transformers[0].GetType().Should().Be(areaPathTransformer.GetType());

            var workItemModel = new SarifWorkItemModel(sarifLog: TestData.CreateSimpleLog(), context);

            context.Transformers[0].Transform(workItemModel);
            workItemModel.Area.Should().BeNull();
        }

        [Fact]
        public void WorkItemFilingContext_IncludeCustomBugFooter()
        {
            var context = new SarifWorkItemContext();

            string customBugFooter = "This text is important to all my bugs.";
            context.AzureDevOpsDescriptionFooter = customBugFooter;
            var workItemModel = new SarifWorkItemModel(sarifLog: TestData.CreateSimpleLog(), context);

            workItemModel.BodyOrDescription.Should().Contain(customBugFooter);
        }

        [Fact]
        public void WorkItemFilingContext_FetchUriSuccessfully()
        {
            var areaPathTransformer = new AreaPathFromUri();

            var context = new SarifWorkItemContext();

            context.AddWorkItemModelTransformer(areaPathTransformer);
            context.Transformers[0].GetType().Should().Be(areaPathTransformer.GetType());

            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();

            var workItemModel = new SarifWorkItemModel(sarifLog, context);

            context.Transformers[0].Transform(workItemModel);
            workItemModel.Area.Should().Be(TestData.FileLocations.Location1);
        }

        private SarifWorkItemContext RoundTripThroughXml(SarifWorkItemContext sarifWorkItemContext)
        {
            string temp = Path.GetTempFileName();

            try
            {
                sarifWorkItemContext.SaveToXml(temp);
                sarifWorkItemContext = new SarifWorkItemContext();
                sarifWorkItemContext.LoadFromXml(temp);
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
            return sarifWorkItemContext;
        }

        public class Munger : SarifWorkItemModelTransformer
        {
            public override SarifWorkItemModel Transform(SarifWorkItemModel workItemModel)
            {
                string newAreaPath = workItemModel.Context.GetProperty(NewAreaPath);

                workItemModel.Area = !string.IsNullOrEmpty(newAreaPath) ? newAreaPath : workItemModel.Area;

                return workItemModel;
            }

            public static PerLanguageOption<string> NewAreaPath { get; } =
                new PerLanguageOption<string>(
                    nameof(Munger), nameof(NewAreaPath),
                    defaultValue: () => { return null; });
        }

        public class AreaPathFromUri : SarifWorkItemModelTransformer
        {
            public override SarifWorkItemModel Transform(SarifWorkItemModel workItemModel)
            {
                string newAreaPath = workItemModel.LocationUris?[0]?.OriginalString;

                workItemModel.Area = !string.IsNullOrEmpty(newAreaPath) ? newAreaPath : workItemModel.Area;

                return workItemModel;
            }
        }
    }
}
