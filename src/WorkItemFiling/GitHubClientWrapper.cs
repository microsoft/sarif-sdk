// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;

namespace Microsoft.WorkItemFiling
{
    /// <summary>
    /// Represents a GitHub project in which work items can be filed.
    /// </summary>
    public class GitHubClientWrapper<T> : FilingClient<T>
    {
        private GitHubClient gitHubClient;

        public GitHubClientWrapper(T configuration) : base(configuration) 
        { 
        }

        public override async Task Connect(string personalAccessToken)
        {
            var credentials = new Credentials(this.AccountOrOrganization, personalAccessToken);
            var credentialsStore = new InMemoryCredentialStore(credentials);

            await Task.Run(() =>
            {
                this.gitHubClient = new GitHubClient(new ProductHeaderValue(this.AccountOrOrganization), credentialsStore);
            });
        }

        public override async Task<IEnumerable<WorkItemModel<T>>> FileWorkItems(IEnumerable<WorkItemModel<T>> workItemFilingMetadata)
        {
            foreach (WorkItemModel<T> workItemModel in workItemFilingMetadata)
            {
                var newIssue = new NewIssue(workItemModel.Title)
                {
                    Body = workItemModel.Body,
                };
                Issue issue = await this.gitHubClient.Issue.Create(this.AccountOrOrganization, this.ProjectOrRepository, newIssue);

                workItemModel.HtmlUri = new Uri(issue.HtmlUrl, UriKind.Absolute);
                workItemModel.Uri = new Uri(issue.Url, UriKind.Absolute);
            }
            return workItemFilingMetadata;
        }
    }
}
