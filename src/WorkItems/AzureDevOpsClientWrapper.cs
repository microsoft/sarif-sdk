// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json;

namespace Microsoft.WorkItems
{
    /// <summary>
    /// Represents an Azure DevOps project in which work items can be filed.
    /// </summary>
    public class AzureDevOpsClientWrapper<T> : FilingClient<T>
    {
        private WorkItemTrackingHttpClient _witClient;

        public AzureDevOpsClientWrapper(T configuration) : base(configuration)
        {
        }

        public override async Task Connect(string personalAccessToken)
        {
            Uri accountUri = new Uri(this.AccountOrOrganization, UriKind.Absolute);

            VssConnection connection = new VssConnection(accountUri, new VssBasicCredential(string.Empty, personalAccessToken));
            await connection.ConnectAsync();

            _witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();
        }

        public override async Task<IEnumerable<WorkItemModel<T>>> FileWorkItems(IEnumerable<WorkItemModel<T>> workItemModels)
        {
            foreach (WorkItemModel<T> workItemModel in workItemModels)
            {
                AttachmentReference attachmentReference = null;
                string attachmentText = workItemModel.Attachment?.Text;
                if (!string.IsNullOrEmpty(attachmentText))
                {
                    using (var stream = new MemoryStream())
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(attachmentText);
                        writer.Flush();
                        stream.Position = 0;
                        try
                        {
                            attachmentReference = await _witClient.CreateAttachmentAsync(stream, fileName: workItemModel.Attachment.Name);
                        }
                        catch
                        {
                            // TBD error handling
                            throw;
                        }
                    }
                }

                var patchDocument = new JsonPatchDocument
                {
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{AzureDevOpsFieldNames.Title}",
                        Value = workItemModel.Title
                    },
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{AzureDevOpsFieldNames.ReproSteps}",
                        Value = workItemModel.Description
                    },
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{AzureDevOpsFieldNames.AreaPath}",
                        Value = workItemModel.Area
                    },
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{AzureDevOpsFieldNames.Tags}",
                        Value = string.Join(",", workItemModel.Tags)
                    }
                };

                if (workItemModel.CustomFields != null)
                {
                    foreach (KeyValuePair<string, string> customField in workItemModel.CustomFields)
                    {
                        patchDocument.Add(new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = $"/fields/{customField.Key}",
                            Value = customField.Value
                        });
                    }
                }

                if (attachmentReference != null)
                {
                    patchDocument.Add(
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = $"/relations/-",
                            Value = new
                            {
                                rel = "AttachedFile",
                                attachmentReference.Url
                            }
                        });
                }

                WorkItem workItem = null;

                try
                {
                    Console.Write($"Creating work item: {workItemModel.Title}");
                    workItem = await _witClient.CreateWorkItemAsync(patchDocument, project: this.ProjectOrRepository, "Bug");
                    Console.WriteLine($": {workItem.Id}: DONE");
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);

                    if (patchDocument != null)
                    {
                        string patchJson = JsonConvert.SerializeObject(patchDocument, Formatting.Indented);
                        Console.Error.WriteLine(patchJson);
                    }

                    continue;
                }

                const string HTML = "html";
                workItemModel.HtmlUri = new Uri(((ReferenceLink)workItem.Links.Links[HTML]).Href, UriKind.Absolute);

                // TODO: populate workItemModel.Uri              
            }

            return workItemModels;
        }
    }
}
