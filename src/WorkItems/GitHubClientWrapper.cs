// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Octokit;

namespace Microsoft.WorkItems
{
    internal class GitHubClientWrapper : IGitHubClientWrapper
    {
        private readonly GitHubClient gitHubClient;

        public GitHubClientWrapper(GitHubClient gitHubClient)
        {
            this.gitHubClient = gitHubClient;
        }

        public Task<Issue> CreateWorkItemAsync(
            string organization,
            string repository,
            NewIssue newissue)
        {
            return this.gitHubClient.Issue.Create(organization, repository, newissue);
        }

        public Task<Issue> UpdateWorkItemAsync(
            string organization,
            string repository,
            int issueNumber,
            IssueUpdate issueUpdate)
        {
            return this.gitHubClient.Issue.Update(organization, repository, issueNumber, issueUpdate);
        }
    }
}