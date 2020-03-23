// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.TeamFoundation.Common;
using Microsoft.WorkItems;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemModel : WorkItemModel<SarifWorkItemContext>
    {
        public Uri LocationUri { get; private set; }

        public SarifWorkItemModel(SarifLog sarifLog, SarifWorkItemContext context = null)
        {
            if (sarifLog == null) { throw new ArgumentNullException(nameof(sarifLog)); }

            this.Context = context ?? new SarifWorkItemContext();

            var visitor = new ExtractAllArtifactLocationsVisitor();
            visitor.VisitSarifLog(sarifLog);
            foreach (ArtifactLocation location in visitor.AllArtifactLocations)
            {
                if (location?.Uri != null)
                {
                    LocationUri = location.Uri;
                    break;
                }
            }

            // Shared GitHub/Azure DevOps concepts

            this.LabelsOrTags = new List<string> { $"default{nameof(this.LabelsOrTags)}" };

            // Note that we provide a simple file name here. The filers will
            // add a prefix to the file name that includes other details,
            // such as the id of the filed item.
            this.Attachment = new Microsoft.WorkItems.Attachment
            {
                Name = "ScanResults.sarif",
                Text = JsonConvert.SerializeObject(sarifLog),
            };

            string title = sarifLog.Runs?[0]?.CreateWorkItemTitle();

            this.Title = title ?? "[ERROR GENERATING TITLE]";

            // TODO: Provide a useful SARIF-derived discussion entry 
            //       for the preliminary filing operation.
            //
            //       https://github.com/microsoft/sarif-sdk/issues/1756
            //
            this.CommentOrDiscussion = $"Default {nameof(this.CommentOrDiscussion)}";

            string descriptionFooter = !string.IsNullOrEmpty(this.Context.BugFooter) ? this.Context.BugFooter : CreateBugFooter(); 
            this.BodyOrDescription = string.Join(Environment.NewLine, sarifLog.CreateWorkItemDescription(LocationUri), descriptionFooter);

            // These properties are Azure DevOps-specific. All ADO work item board
            // area paths are rooted by the project name, as are iterations.
            this.Area = this.RepositoryOrProject;
            this.Iteration = this.RepositoryOrProject;

            // Milestone is a shared concept between GitHub and AzureDevOps. For both
            // environments this field is an open-ended text field. As such, there is
            // no useful default. An empty string here will prompt filers to skip
            // updating this field.
            this.Milestone = string.Empty;

            // No defaults are provided for custom fields. This dictionary is used
            // to provide values for non-standard fields as defined in an Azure
            // DevOps work item template. Because this data by definition addresses
            // non-generalized needs, there are no useful defaults we can provide.
            //
            // this.CustomFields
        }

        private string CreateBugFooter()
        {
            if(this.Context.CurrentProvider == FilingClient.SourceControlProvider.AzureDevOps)
            {
                StringBuilder azureDevOpsFooter = new StringBuilder();
                azureDevOpsFooter.Append(@"<br><br>Other viewing options:<br>");
                azureDevOpsFooter.AppendLine();
                azureDevOpsFooter.AppendLine();
                azureDevOpsFooter.Append(@"<li>Examine the complete <a href=""https://dev.azure.com/office/Office/_componentGovernance/Office?_a=alerts&typeId=1731351&alerts-view-option=active"">CG Scan</a> for this repository.</li>");
                azureDevOpsFooter.AppendLine();
                azureDevOpsFooter.Append(@"<li>Load the attached log file in the <a href=""https://marketplace.visualstudio.com/_apis/public/gallery/publishers/WDGIS/vsextensions/MicrosoftSarifViewer/2.1.7/vspackage"">CG Scan</a> Visual Studio SARIF add-in.</li>");
                azureDevOpsFooter.AppendLine();
                azureDevOpsFooter.Append(@"<li>Load the attached log file in the <a href=""https://marketplace.visualstudio.com/items?itemName=MS-SarifVSCode.sarif-viewer"">CG Scan</a> VS Code SARIF extension.</li>");
                return azureDevOpsFooter.ToString();
            }
            else
            {
                return @"Details for the above issues can be found in the attachment filed with this issue.";
            }
        }
    }
}
