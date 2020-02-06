// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Represents a GitHub project in which work items can be filed.
    /// </summary>
    public class GitHubClient : FilingClient
    {
        public override Task<IEnumerable<WorkItemModel>> FileWorkItems(IEnumerable<WorkItemModel> workItemModels)
        {
            throw new NotImplementedException();
        }
    }
}
