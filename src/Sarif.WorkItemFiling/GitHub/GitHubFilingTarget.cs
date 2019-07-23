// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Grouping;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling.GitHub
{
    /// <summary>
    /// Represents a GitHub project in which work items can be filed.
    /// </summary>
    public class GitHubFilingTarget : FilingTarget
    {
        private readonly IGitHubClient _gitHubClient;

        public GitHubFilingTarget(IGitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
        }

        public override Task<IEnumerable<ResultGroup>> FileWorkItems(IEnumerable<ResultGroup> resultGroups)
        {
            throw new NotImplementedException();
        }
    }
}
