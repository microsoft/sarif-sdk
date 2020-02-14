// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.WorkItems;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public static class Extensions
    {
        public static WorkItemModel<SarifWorkItemContext> CreateWorkItemModel(
            this SarifLog sarifLog, 
            SarifWorkItemContext rootContext)
        {            
            var sarifWorkItemData = new SarifWorkItemContext(rootContext);
            sarifWorkItemData.InitializeFromLog(sarifLog);

            // For default ADO work item boards, the project name is repurposed
            // to provide a default area path and iteration
            var model = new WorkItemModel<SarifWorkItemContext>()
            {
                Area = "Area",
                Attachment = new Microsoft.WorkItems.Attachment
                {
                    Name = "AttachedResults.sarif",
                    Text = JsonConvert.SerializeObject(sarifLog),
                },
                Context = sarifWorkItemData,
                Description = "Description",
                Discussion = "Discussion",
                Title = "Title",
                Iteration = "Iteration",
                Tags = new List<string> { "tag"}                 
            };

            return model;
        }
    }
}
