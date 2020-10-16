// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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

        public Task<WorkItem> UpdateWorkItemAsync(
            JsonPatchDocument document,
            int id,
            bool? validateOnly = null,
            bool? bypassRules = null,
            bool? suppressNotifications = null,
            WorkItemExpand? expand = null,
            object userState = null,
            CancellationToken cancellationToken = default)
        {
            return this.workItemTrackingHttpClient.UpdateWorkItemAsync(document, id, validateOnly, bypassRules, suppressNotifications, expand, userState, cancellationToken);
        }

        public Task<WorkItem> GetWorkItemAsync(
            string project,
            int id,
            IEnumerable<string> fields = null,
            DateTime? asOf = null,
            WorkItemExpand? expand = null,
            object userState = null,
            CancellationToken cancellationToken = default)
        {
            return this.workItemTrackingHttpClient.GetWorkItemAsync(project, id, fields, asOf, expand, userState, cancellationToken);
        }

        public void Dispose()
        {
            if (this.workItemTrackingHttpClient != null)
            {
                this.workItemTrackingHttpClient.Dispose();
                this.workItemTrackingHttpClient = null;
            }
        }
    }
}