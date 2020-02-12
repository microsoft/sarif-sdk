// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.WorkItemFiling;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public static class Extensions
    {
        public static WorkItemModel<SarifWorkItemData> CreateWorkItemModel(this SarifLog sarifLog, string projectName)
        {
            var sarifWorkItemData = new SarifWorkItemData();
            sarifWorkItemData.InitializeFromLog(sarifLog);

            // For default ADO work item boards, the project name is repurposed
            // to provide a default area path and iteration
            var model = new WorkItemModel<SarifWorkItemData>()
            {
                Area = projectName,
                Attachment = new Microsoft.WorkItemFiling.Attachment
                {
                    Name = "AttachedResults.sarif",
                    Text = JsonConvert.SerializeObject(sarifLog),
                },
                Data = sarifWorkItemData,
                Description = "Description",
                Discussion = "Discussion",
                Title = "Title",
                Iteration = projectName,
                Tags = new List<string> { "tag"}                 
            };

            return model;
        }
    }
}
