// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling.GitHub
{
    /// <summary>
    /// Represents a GitHub project to which work items can be filed.
    /// </summary>
    public class GitHubFilingTarget : FilingTarget
    {
        private readonly IGitHubClient _gitHubClient;

        public GitHubFilingTarget(IGitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
        }

        public override Task<IEnumerable<Result>> FileWorkItems(IEnumerable<Result> results)
        {
            throw new NotImplementedException();
        }
    }
}
