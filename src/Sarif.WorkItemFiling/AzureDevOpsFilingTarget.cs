// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Represents an Azure DevOps project in which work items can be filed.
    /// </summary>
    public class AzureDevOpsFilingTarget : FilingTarget
    {
        public override Task Connect(Uri projectUri)
        {
            return Task.CompletedTask;
        }

        public override async Task<IEnumerable<ResultGroup>> FileWorkItems(IEnumerable<ResultGroup> resultGroups)
        {
            foreach(ResultGroup resultGroup in resultGroups)
            {
                var uris = new List<Uri>(new[] { new Uri("https://workitem.example.com/" + Guid.NewGuid().ToString()) });
                foreach(Result result in resultGroup.Results)
                {
                    result.WorkItemUris = uris;
                }
            }

            return await Task.FromResult(resultGroups);
        }
    }
}
