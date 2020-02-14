// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using Microsoft.WorkItems;
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
            context.WorkItemModelTransformers[0].GetType().Should().Be(munger.GetType());

            context = RoundTripThroughXml(context);
            context.WorkItemModelTransformers[0].GetType().Should().Be(munger.GetType());

            string newAreaPath = Guid.NewGuid().ToString();
            context.SetProperty(Munger.NewAreaPath, newAreaPath);

            var workItemModel = new WorkItemModel<SarifWorkItemContext>()
            {
                Context = context
            };

            context.WorkItemModelTransformers[0].Transform(workItemModel);
            workItemModel.Area.Should().Be(newAreaPath);
        }

        private SarifWorkItemContext RoundTripThroughXml(SarifWorkItemContext propertiesDictionary)
        {
            string temp = Path.GetTempFileName();

            try
            {
                propertiesDictionary.SaveToXml(temp);
                propertiesDictionary.LoadFromXml(temp);
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
            return propertiesDictionary;
        }

        public class Munger : IWorkItemModelTransformer<SarifWorkItemContext>
        {            
            public void Transform(WorkItemModel<SarifWorkItemContext> workItemModel)
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
