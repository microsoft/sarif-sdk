// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Represents an Azure DevOps project in which work items can be filed.
    /// </summary>
    public class AzureDevOpsFilingTarget : FilingTarget
    {
        private WorkItemTrackingHttpClient _witClient;
        private string _projectName;

        // TEMPORARY: To demonstrate filing multiple bugs.
        private int _bugNumber = 1;

        public override async Task Connect(Uri projectUri, string personalAccessToken)
        {
            string projectUriString = projectUri.OriginalString;
            int lastSlashIndex = projectUriString.LastIndexOf('/');
            _projectName = lastSlashIndex > 0 && lastSlashIndex < projectUriString.Length - 1
                ? projectUriString.Substring(lastSlashIndex + 1)
                : throw new ArgumentException($"Cannot parse project name from URI {projectUriString}");

            string accountUriString = projectUriString.Substring(0, lastSlashIndex);
            Uri accountUri = new Uri(accountUriString, UriKind.Absolute);

            VssConnection connection = new VssConnection(accountUri, new VssBasicCredential(string.Empty, personalAccessToken));
            await connection.ConnectAsync();

            _witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();
        }

        public override async Task<IEnumerable<ResultGroup>> FileWorkItems(IEnumerable<ResultGroup> resultGroups)
        {
            foreach (ResultGroup resultGroup in resultGroups)
            {
                var patchDocument = new JsonPatchDocument
                {
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.Title",
                        Value = $"Bug #{_bugNumber} was added programmatically!"
                    }
                };

                ++_bugNumber;

                WorkItem workItem = await _witClient.CreateWorkItemAsync(patchDocument, project: _projectName, "Issue");
                foreach (Result result in resultGroup.Results)
                {
                    result.WorkItemUris = new List<Uri> { new Uri(workItem.Url, UriKind.Absolute) };
                }
            }

            return resultGroups;
        }
    }
}
