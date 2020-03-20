﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Visitors;
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


            // TODO: Provide a useful SARIF-derived issue body. Note that there
            //       is a client-specific consideration here in that there is much
            //       less pressure to provide an ADO rendering of the scan results 
            //       in the bug body, as we can integrate the SARIF results viewing
            //       web control in that environment. So for this scenario, a very
            //       simple boilerplate text could be sufficient. For GH, we would
            //       prefer the issue body to provide more details. Another clear
            //       distinction centers on differences in rendering format
            //       (markdown vs. HTML). We only have a single tracking issue below - 
            //       in the event, this work is likely to be broken aparts into 
            //       multiple tasks, each of which may entail breaking changes to 
            //       models/API.
            //
            //       https://github.com/microsoft/sarif-sdk/issues/1757
            //
            this.BodyOrDescription = $"Default {nameof(this.BodyOrDescription)}";

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
    }
}
