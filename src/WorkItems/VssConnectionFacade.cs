// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Microsoft.WorkItems
{
    internal class VssConnectionFacade : IVssConnection
    {
        private VssConnection connection;

        async public Task ConnectAsync(Uri accountUri, string personalAccessToken)
        {
            this.connection = new VssConnection(accountUri, new VssBasicCredential(string.Empty, personalAccessToken));
            await this.connection.ConnectAsync();
        }

        async public Task<IWorkItemTrackingHttpClient> GetClientAsync()
        {
            WorkItemTrackingHttpClient workItemTrackingHttpClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();
            return new WorkItemTrackingHttpClientWrapper(workItemTrackingHttpClient);
        }

        public void Dispose()
        {
            if (this.connection != null)
            {
                this.connection.Disconnect();
                this.connection.Dispose();
                this.connection = null;
            }
        }
    }
}