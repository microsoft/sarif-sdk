// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.WorkItems.Logging;
using Newtonsoft.Json;

namespace Microsoft.WorkItems
{
    /// <summary>
    /// Represents an Azure DevOps project in which work items can be filed.
    /// </summary>
    public class AzureDevOpsFilingClient : FilingClient
    {
        // We use these fields for mock object injection when unit testing.
        internal IVssConnection _vssConection;
        internal IWorkItemTrackingHttpClient _witClient;

        public Uri AccountUri => new Uri(string.Format("https://dev.azure.com/" + this.AccountOrOrganization));

        public override async Task Connect(string personalAccessToken)
        {
            using (Logger.BeginScopeContext(nameof(Connect)))
            {
                _vssConection = _vssConection ?? new VssConnectionFacade();

                await _vssConection.ConnectAsync(this.AccountUri, personalAccessToken);

                _witClient = await _vssConection.GetClientAsync();
            }
        }

        public override async Task<IEnumerable<WorkItemModel>> FileWorkItems(IEnumerable<WorkItemModel> workItemModels)
        {
            foreach (WorkItemModel workItemModel in workItemModels)
            {
                switch (workItemModel.Operation)
                {
                    case WorkItemOperation.Create:
                        await CreateWorkItem(workItemModel);
                        break;
                    case WorkItemOperation.Update:
                        await UpdateWorkItem(workItemModel);
                        break;
                }
            }

            return workItemModels;
        }

        public async Task<WorkItemModel> CreateWorkItem(WorkItemModel workItemModel)
        {
            using (Logger.BeginScopeContext(nameof(CreateWorkItem)))
            {
                //TODO: Provide helper that generates useful attachment name from filed bug.
                //      This helper should be common to both ADO and GH filers. The implementation
                //      should work like this: the filer should prefix the proposed file name with
                //      the account and project and number of the filed work item. So an attachment
                //      name of Scan.sarif would be converted tO
                //
                //         MyAcct_MyProject_WorkItem1000_Scan.sarif
                //
                //      The GH filer may prefer to use 'issue' instead:
                //
                //         myowner_my-repo_Issue1000_Scan.sarif
                //
                //      The common helper should preserve casing choices in the account/owner and
                //      project/repo information that's provided.
                //
                //      Obviously, this proposal requires a change below to first file the bug,
                //      then compute the file name and add the attachment.
                //
                //      https://github.com/microsoft/sarif-sdk/issues/1753

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
                            attachmentReference = await _witClient.CreateAttachmentAsync(
                                stream,
                                fileName: workItemModel.Attachment.Name);
                        }
                        catch
                        {
                            // Implement simple, sensible logging mechanism
                            //
                            // https://github.com/microsoft/sarif-sdk/issues/1771
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
                        Path = $"/fields/{AzureDevOpsFieldNames.Area}",
                        Value = workItemModel.Area ?? workItemModel.RepositoryOrProject
                    },
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{AzureDevOpsFieldNames.Tags}",
                        Value = string.Join(",", workItemModel.LabelsOrTags)
                    },
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{AzureDevOpsFieldNames.ReproSteps}",
                        Value = workItemModel.BodyOrDescription
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
                    // TODO: Make work item kind configurable for Azure DevOps filer
                    //
                    // https://github.com/microsoft/sarif-sdk/issues/1770
                    string workItemKind = "Bug";

                    Logger.LogInformation($"Creating work item: {workItemModel.Title}");
                    workItem = await _witClient.CreateWorkItemAsync(patchDocument, project: workItemModel.RepositoryOrProject, workItemKind);
                    workItemModel.Uri = new Uri(workItem.Url, UriKind.Absolute);

                    workItemModel.HtmlUri = new Uri(((ReferenceLink)workItem.Links.Links["html"]).Href, UriKind.Absolute);
                    Logger.LogInformation($"CREATED: {workItemModel.HtmlUri}");
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    this.Logger.LogError(e, "Creating work item: {workItemModel.Title}", workItemModel.Title);

                    if (patchDocument != null)
                    {
                        string patchJson = JsonConvert.SerializeObject(patchDocument, Formatting.Indented);
                        Console.Error.WriteLine(patchJson);
                    }
                }

                return workItemModel;
            }
        }

        public async Task<WorkItemModel> UpdateWorkItem(WorkItemModel workItemModel)
        {
            using (Logger.BeginScopeContext(nameof(UpdateWorkItem)))
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
                            attachmentReference = await _witClient.CreateAttachmentAsync(
                                stream,
                                fileName: workItemModel.Attachment.Name);
                        }
                        catch
                        {
                            // Implement simple, sensible logging mechanism
                            //
                            // https://github.com/microsoft/sarif-sdk/issues/1771
                            throw;
                        }
                    }
                }

                var patchDocument = new JsonPatchDocument();

                if (attachmentReference != null)
                {
                    patchDocument.Add(
                        new JsonPatchOperation
                        {
                            Operation = Operation.Remove,
                            Path = $"/relations/0"
                        });

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

                    patchDocument.Add(
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = $"/fields/{AzureDevOpsFieldNames.History}",
                            Value = workItemModel.CommentOrDiscussion
                        });

                    patchDocument.Add(
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = $"/fields/{AzureDevOpsFieldNames.ReproSteps}",
                            Value = workItemModel.BodyOrDescription
                        });
                }

                WorkItem workItem = null;

                try
                {
                    if (int.TryParse(workItemModel.Uri.OriginalString.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault(), out int workItemId))
                    {
                        Logger.LogInformation($"Updating work item id: {workItemId}");
                        workItem = await _witClient.UpdateWorkItemAsync(patchDocument, id: workItemId);

                        Logger.LogInformation($"UPDATED: {workItemModel.Uri}");
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    this.Logger.LogError(e, "Updating work item: {workItemModel.Title}", workItemModel.Title);

                    if (patchDocument != null)
                    {
                        string patchJson = JsonConvert.SerializeObject(patchDocument, Formatting.Indented);
                        Console.Error.WriteLine(patchJson);
                    }
                }

                return workItemModel;
            }
        }

        public override async Task<WorkItemModel> GetWorkItemMetadata(WorkItemModel workItemModel)
        {
            using (Logger.BeginScopeContext(nameof(GetWorkItemMetadata)))
            {
                if (workItemModel.Uri != null
                && int.TryParse(workItemModel.Uri.OriginalString.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault(), out int workItemId))
                {
                    try
                    {
                        WorkItem workItem = await _witClient.GetWorkItemAsync(workItemModel.RepositoryOrProject, workItemId);
                        workItemModel.State = $"{workItem.Fields[AzureDevOpsFieldNames.State]}";

                        if (workItem.Fields.ContainsKey(AzureDevOpsFieldNames.Tags))
                        {
                            string tagsList = $"{workItem.Fields[AzureDevOpsFieldNames.Tags]}";
                            if (!string.IsNullOrWhiteSpace(tagsList))
                            {
                                string[] tags = tagsList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string tag in tags)
                                {
                                    workItemModel.LabelsOrTags.Add(tag.Trim());
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e);
                        this.Logger.LogError(e, "Getting work item: {workItemModel.Uri.OriginalString}", workItemModel.Uri.OriginalString);

                        // Two scenarios for this exception, either we do not have access to the work item or the work item 
                        // no longer exists.  In either case, setting Uri to null to pass through for further processing.
                        if (workItemModel != null)
                        {
                            workItemModel.Uri = null;
                        }
                    }
                }
            }

            return workItemModel;
        }
        public override void Dispose()
        {
            this._vssConection?.Dispose();
            this._vssConection = null;

            this._witClient?.Dispose();
            this._witClient = null;
        }
    }
}
