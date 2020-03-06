// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Microsoft.WorkItems
{
    internal class WorkItemTrackingHttpClientWrapper : IWorkItemTrackingHttpClient
    {
        private WorkItemTrackingHttpClient workItemTrackingHttpClient;

        public WorkItemTrackingHttpClientWrapper(WorkItemTrackingHttpClient workItemTrackingHttpClient)
        {
            this.workItemTrackingHttpClient = workItemTrackingHttpClient;
        }

        public Task<AttachmentReference> CreateAttachmentAsync(
            MemoryStream stream, 
            string fileName = null,
            string uploadType = null,
            string areaPath = null,
            object userState = null,
            CancellationToken cancellationToken = default)
        {
            return this.workItemTrackingHttpClient.CreateAttachmentAsync(stream, fileName, uploadType, areaPath, userState, cancellationToken);
        }

        public Task<WorkItem> CreateWorkItemAsync(
            JsonPatchDocument document, 
            string project, 
            string type, 
            bool? validateOnly = null,
            bool? bypassRules = null,
            bool? suppressNotifications = null,
            object userState = null,
            CancellationToken cancellationToken = default)
        {
            return this.workItemTrackingHttpClient.CreateWorkItemAsync(document, project, type, validateOnly, bypassRules, suppressNotifications, userState, cancellationToken);
        }
    }
}