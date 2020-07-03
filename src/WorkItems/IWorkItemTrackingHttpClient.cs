// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Microsoft.WorkItems
{
    public interface IWorkItemTrackingHttpClient : IDisposable
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
        /// Wraps Microsoft.TeamFoundation.WorkItemTracking.WebApi.CreateWorkItemAsync.
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

        /// <summary>
        /// Wraps Microsoft.TeamFoundation.WorkItemTracking.WebApi.UpdateWorkItemAsync.
        /// Updates a single work item.
        /// </summary>
        Task<WorkItem> UpdateWorkItemAsync(
            JsonPatchDocument document,
            int id,
            bool? validateOnly = null,
            bool? bypassRules = null,
            bool? suppressNotifications = null,
            WorkItemExpand? expand = null,
            object userState = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Wraps Microsoft.TeamFoundation.WorkItemTracking.WebApi.GetWorkItemAsync.
        /// Gets a single work item.
        /// </summary>
        Task<WorkItem> GetWorkItemAsync(
            string project,
            int id,
            IEnumerable<string> fields = null,
            DateTime? asOf = null,
            WorkItemExpand? expand = null,
            object userState = null,
            CancellationToken cancellationToken = default);
    }
}