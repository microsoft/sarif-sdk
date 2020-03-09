// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;

namespace Microsoft.WorkItems
{
    /// <summary>
    /// Represents a GitHub project in which work items can be filed.
    /// </summary>
    public class GitHubFilingClient : FilingClient
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

        public override async Task<IEnumerable<WorkItemModel>> FileWorkItems(IEnumerable<WorkItemModel> workItemModels)
        {
            foreach (WorkItemModel workItemModel in workItemModels)
            {
                var newIssue = new NewIssue(workItemModel.Title)
                {
                    Body = workItemModel.BodyOrDescription,
                };

                Issue issue = await this.gitHubClient.Issue.Create(
                    this.AccountOrOrganization,
                    this.ProjectOrRepository,
                    newIssue);

                // TODO: Can we collapse GH issue creation to a single operation?
                // 
                // https://github.com/microsoft/sarif-sdk/issues/1790

                if (workItemModel.LabelsOrTags?.Count != 0)
                {
                    var issueUpdate = new IssueUpdate();

                    foreach (string tag in workItemModel.LabelsOrTags)
                    {
                        issueUpdate.AddLabel(tag);
                    }

                    await this.gitHubClient.Issue.Update(
                        this.AccountOrOrganization,
                        this.ProjectOrRepository,
                        issue.Number,
                        issueUpdate);
                }

                workItemModel.Uri = new Uri(issue.Url, UriKind.Absolute);
                workItemModel.HtmlUri = new Uri(issue.HtmlUrl, UriKind.Absolute);

                // TODO: Extend GitHub issue filer to add file attachments 
                // https://github.com/microsoft/sarif-sdk/issues/1754

                // TODO: Provide helper that generates useful attachment name from filed bug.
                //       This helper should be common to both ADO and GH filers. The implementation 
                //       should work like this: the filer should prefix the proposed file name with
                //       the account and project and number of the filed work item. So an attachment
                //       name of Scan.sarif would be converted tO
                // 
                //          MyAcct_MyProject_WorkItem1000_Scan.sarif
                //
                //       The GH filer may prefer to use 'issue' instead:
                // 
                //          myowner_my-repo_Issue1000_Scan.sarif
                //
                //       The common helper should preserve casing choices in the account/owner and
                //       project/repo information that's provided.
                //    
                //       https://github.com/microsoft/sarif-sdk/issues/1753
            }
            return workItemModels;
        }
    }
}