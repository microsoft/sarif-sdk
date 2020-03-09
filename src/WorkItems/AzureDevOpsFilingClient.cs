// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json;

namespace Microsoft.WorkItems
{
    /// <summary>
    /// Represents an Azure DevOps project in which work items can be filed.
    /// </summary>
    public class AzureDevOpsFilingClient : FilingClient
    {
        internal IWorkItemTrackingHttpClient _witClient;

        // We use this field for mock object injection when unit testing.
        internal IVssConnection vssConection;

        public Uri AccountUri => new Uri(string.Format("https://dev.azure.com/" + this.AccountOrOrganization));

        public override async Task Connect(string personalAccessToken)
        {
            vssConection = vssConection ?? new VssConnectionWrapper();

            await vssConection.ConnectAsync(this.AccountUri, personalAccessToken);
            
            _witClient = await vssConection.GetClientAsync();
        }

        public override async Task<IEnumerable<WorkItemModel>> FileWorkItems(IEnumerable<WorkItemModel> workItemModels)
        {
            foreach (WorkItemModel workItemModel in workItemModels)
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

                    Console.Write($"Creating work item: {workItemModel.Title}");
                    workItem = await _witClient.CreateWorkItemAsync(patchDocument, project: this.ProjectOrRepository, workItemKind);
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

                // TODO: ADO work item filer should populate the raw URI (in addition to HtmlUri) 
                //
                //       https://github.com/microsoft/sarif-sdk/issues/1773
            }

            return workItemModels;
        }

        public override void Dispose()
        {
            if (this.vssConection != null)
            {
                this.vssConection.Dispose();
                this.vssConection = null;
            }

            if (this._witClient != null)
            {
            }
        }
    }
}
