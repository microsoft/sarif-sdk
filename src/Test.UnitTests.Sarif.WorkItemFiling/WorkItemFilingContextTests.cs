// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.WorkItemFiling;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class WorkItemFilingContextTests
    {
        [Fact]
        public void WorkItemFilingContext_RoundTripsWorkItemModelTransformers()
        {
            var configuration = new WorkItemFilingConfiguration<MungerData>();

            var munger = new Munger();

            configuration.AddWorkItemModelTransformer(munger);
            configuration.WorkItemModelTransformers[0].GetType().Should().Be(munger.GetType());

           configuration = RoundTripThroughXml(configuration);
           configuration.WorkItemModelTransformers[0].GetType().Should().Be(munger.GetType());
        }

        private WorkItemFilingConfiguration<MungerData> RoundTripThroughXml(WorkItemFilingConfiguration<MungerData> propertiesDictionary)
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

        public class MungerData
        {
            public string NewArea { get; set; }
            public string NewDescription { get; set; }
            public string NewTitle { get; set; }
        }

        public class Munger : IWorkItemModelTransformer<MungerData>
        {            
            public void Transform(WorkItemModel<MungerData> workItemModel)
            {
                workItemModel.Area = !string.IsNullOrEmpty(workItemModel.Data.NewArea)
                    ? workItemModel.Data.NewArea : workItemModel.Area;

                workItemModel.Title = !string.IsNullOrEmpty(workItemModel.Data.NewTitle)
                    ? workItemModel.Data.NewTitle : workItemModel.Title;
                
                workItemModel.Description = !string.IsNullOrEmpty(workItemModel.Data.NewDescription)
                    ? workItemModel.Data.NewDescription : workItemModel.Description;
            }
        }
    }
}
