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
    public class GitHubFilingTarget : FilingTarget
    {
        public override Task<IEnumerable<WorkItemMetadata>> FileWorkItems(IEnumerable<WorkItemMetadata> workItemMetadata)
        {
            throw new NotImplementedException();
        }
    }
}
