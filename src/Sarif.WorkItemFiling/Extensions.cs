// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public static class Extensions
    {
        public static WorkItemModel CreateWorkItemModel(this SarifLog sarifLog, string projectName)
        {
            // For default ADO work item boards, the project name is repurposed
            // to provide a default area path and iteration
            WorkItemModel model = new WorkItemModel()
            {
                AdditionalData = sarifLog,
                Area = projectName,
                Attachment = new WorkItemFiling.Attachment
                {
                    Name = "AttachedResults.sarif",
                    Text = JsonConvert.SerializeObject(sarifLog),
                },
                Description = "Description",
                Discussion = "Discussion",
                Title = "Title",
                Iteration = projectName,
                Tags = new List<string> { "tag"}                 
            };

            return model;
        }

        public static string GetProjectOrRepositoryName(this Uri filingHostUri)
        {
            string projectUriString = filingHostUri.OriginalString;
            int lastSlashIndex = projectUriString.LastIndexOf('/'); 

            string projectName = lastSlashIndex > 0 && lastSlashIndex < projectUriString.Length - 1
                ? projectUriString.Substring(lastSlashIndex + 1)
                : throw new ArgumentException($"Cannot parse project name from URI {projectUriString}");

            return WebUtility.UrlDecode(projectName);
        }

        public static string GetAccountOrOrganizationName(this Uri filingHostUri)
        {
            string projectUriString = filingHostUri.OriginalString;
            int lastSlashIndex = projectUriString.LastIndexOf('/');

            string accountUriString = projectUriString.Substring(0, lastSlashIndex);

            return accountUriString;
        }
    }
}
