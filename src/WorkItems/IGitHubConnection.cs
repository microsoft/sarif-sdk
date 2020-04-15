// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Octokit;
using System;
using System.Threading.Tasks;

namespace Microsoft.WorkItems
{
    /// <summary>
    /// This interface allows for mocking of the low-level Github connection class.
    /// </summary>
    internal interface IGitHubConnection
    {
        /// <summary>
        /// Provide for the instantiation of the GitHub connection instance.
        /// </summary>
        /// <param name="organization">The GitHuub organization URI for the connection.</param>
        /// <param name="personalAccessToken">A personal access token with sufficient permissions to establish the connection.</param>
        /// <returns></returns>
        Task<IGitHubClientWrapper> ConnectAsync(string organization, string personalAccessToken);
    }
}