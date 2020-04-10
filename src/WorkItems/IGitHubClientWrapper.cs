// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Octokit;
using System.Threading.Tasks;

namespace Microsoft.WorkItems
{
    public interface IGitHubClientWrapper
    {
        /// <summary>
        /// Wraps GitHubClient.Issue.Create.
        /// Creates a single work item.
        /// </summary>
        public Task<Issue> CreateWorkItemAsync(
                    string organization,
                    string repository,
                    NewIssue newissue);

        /// <summary>
        /// Wraps GitHubClient.Issue.Update
        /// Creates a single work item.
        /// </summary>
        public Task<Issue> UpdateWorkItemAsync(
            string organization,
            string repository,
            int issueNumber,
            IssueUpdate issueUpdate);
    }
}