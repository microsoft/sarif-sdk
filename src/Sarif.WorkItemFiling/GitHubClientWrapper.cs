// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Json.Schema;
using Octokit;
using Octokit.Internal;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Represents a GitHub project in which work items can be filed.
    /// </summary>
    public class GitHubClientWrapper : FilingClient
    {
        private GitHubClient gitHubClient;

        public override async Task Connect(string personalAccessToken)
        {
            var credentials = new Credentials(this.AccountOrOrganization, personalAccessToken);
            var credentialsStore = new InMemoryCredentialStore(credentials);

            await Task.Run(() =>
            {
                this.gitHubClient = new GitHubClient(new ProductHeaderValue(this.AccountOrOrganization), credentialsStore);
            });
        }
        public override async Task<IEnumerable<WorkItemModel>> FileWorkItems(IEnumerable<WorkItemModel> workItemFilingMetadata)
        {
            foreach (WorkItemModel workItemModel in workItemFilingMetadata)
            {
                var newIssue = new NewIssue(workItemModel.Title)
                {
                    Body = workItemModel.Body,
                };
                Issue issue = await this.gitHubClient.Issue.Create("michaelcfanning", "bug-dummy", newIssue);

                SarifLog sarifLog = (SarifLog)workItemModel.AdditionalData;
                foreach (Result result in sarifLog.Runs[0].Results)
                {
                    result.WorkItemUris = new List<Uri> { new Uri(issue.HtmlUrl, UriKind.Absolute) };
                }
            }

            return workItemFilingMetadata;
        }
    }
}
