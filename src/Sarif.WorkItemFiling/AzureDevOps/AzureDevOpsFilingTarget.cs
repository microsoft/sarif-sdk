// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling.AzureDevOps
{
    /// <summary>
    /// Represents an Azure DevOps project to which work items can be filed.
    /// </summary>
    public class AzureDevOpsFilingTarget : FilingTarget
    {
        private readonly IAzureDevOpsClient _azureDevOpsClient;

        public AzureDevOpsFilingTarget(IAzureDevOpsClient azureDevOpsClient)
        {
            _azureDevOpsClient = azureDevOpsClient ?? throw new ArgumentNullException(nameof(azureDevOpsClient));
        }

        public override Task<IEnumerable<ResultGroup>> FileWorkItems(IEnumerable<ResultGroup> resultGroups)
        {
            throw new System.NotImplementedException();
        }
    }
}
