// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Octokit;
using Octokit.Internal;

namespace Microsoft.WorkItems
{
    internal class GitHubConnectionFacade : IGitHubConnection
    {
        async public Task<IGitHubClientWrapper> ConnectAsync(string organization, string personalAccessToken)
        {

            var credentials = new Credentials(personalAccessToken);
            var credentialsStore = new InMemoryCredentialStore(credentials);
            GitHubClientWrapper gitHubClientWrapper;

            gitHubClientWrapper = await Task.Run(() =>
            {
                var gitHubClient = new GitHubClient(new ProductHeaderValue(organization), credentialsStore);
                return new GitHubClientWrapper(gitHubClient);
            });

            return gitHubClientWrapper;
        }
    }
}