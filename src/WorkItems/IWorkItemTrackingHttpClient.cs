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
    public interface IWorkItemTrackingHttpClient
    {
        /// <summary>
        /// Wraps Microsoft.TeamFoundation.WorkItemTracking.WebApi.CreateAttachmentAsync.
        /// </summary>
        Task<AttachmentReference> CreateAttachmentAsync(
            MemoryStream stream,
            string fileName,
            string uploadType = null,
            string areaPath = null,
            object userState = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Wraps Microsoft.TeamFoundation.WorkItemTracking.WebApi.CreateAttachmentAsync.
        /// Creates a single work item.
        /// </summary>
        Task<WorkItem> CreateWorkItemAsync(
            JsonPatchDocument document, 
            string project, 
            string type, 
            bool? validateOnly = null, 
            bool? bypassRules = null, 
            bool? suppressNotifications = null, 
            object userState = null, 
            CancellationToken cancellationToken = default);
    }
}