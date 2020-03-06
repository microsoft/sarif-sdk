// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
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

            var workItemModel = new SarifWorkItemModel(sarifLog: null, context);

            context.Transformers[0].Transform(workItemModel);
            workItemModel.Area.Should().Be(newAreaPath);
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
            public override void Transform(SarifWorkItemModel workItemModel)
            {
                string newAreaPath = workItemModel.Context.GetProperty(NewAreaPath);

                workItemModel.Area = !string.IsNullOrEmpty(newAreaPath) ? newAreaPath : workItemModel.Area;
            }

            public static PerLanguageOption<string> NewAreaPath { get; } =
                new PerLanguageOption<string>(
                    nameof(Munger), nameof(NewAreaPath),
                    defaultValue: () => { return null; });
        }
    }
}
