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
        public static WorkItemModel CreateWorkItemModel(this SarifLog sarifLog)
        {
            WorkItemModel metadata = new WorkItemModel()
            {
                AdditionalData = sarifLog,
                Area = "AreaPath",
                Attachment = new WorkItemFiling.Attachment
                {
                    Name = "AttachedResults.sarif",
                    Text = JsonConvert.SerializeObject(sarifLog),
                },
                Description = "Description",
                Title = "Title",
                Tags = new List<string> { "tag"}                 
            };

            return metadata;
        }

        public static string GetProjectName(this Uri projectUri)
        {
            string projectUriString = projectUri.OriginalString;
            int lastSlashIndex = projectUriString.LastIndexOf('/');

            string projectName = lastSlashIndex > 0 && lastSlashIndex < projectUriString.Length - 1
                ? projectUriString.Substring(lastSlashIndex + 1)
                : throw new ArgumentException($"Cannot parse project name from URI {projectUriString}");

            return WebUtility.UrlDecode(projectName);
        }

        public static string GetAccountName(this Uri projectUri)
        {
            string projectUriString = projectUri.OriginalString;
            int lastSlashIndex = projectUriString.LastIndexOf('/');

            string accountUriString = projectUriString.Substring(0, lastSlashIndex);

            return accountUriString;
        }
    }
}
