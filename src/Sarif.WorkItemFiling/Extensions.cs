// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public static class Extensions
    {
        public static WorkItemFilingMetadata CreateWorkItemFilingMetadata(this SarifLog sarifLog, string workItemProjectName)
        {
            WorkItemFilingMetadata metadata = new WorkItemFilingMetadata()
            {
                Object = sarifLog,
                Attachment = new WorkItemFiling.Attachment
                {
                    Name = "AttachedResults.sarif",
                    Text = JsonConvert.SerializeObject(sarifLog)
                }
            };

            ArtifactLocation artifactLocation = sarifLog.Runs[0].Artifacts[0].Location;

            // BUG: GetProperty doesn't unencode string values
            string organization = artifactLocation.GetProperty("OrganizationName");
            string buildDefinitionId = artifactLocation.GetProperty<int>("BuildDefinitionId").ToString();
            string projectName = artifactLocation.GetProperty("ProjectName");
            string areaPath = $@"{workItemProjectName}" + artifactLocation.GetProperty("AreaPath").Replace(@"\\", @"\");
            string buildDefinitionName = artifactLocation.GetProperty("BuildDefinitionName");

            Result result = sarifLog.Runs[0].Results[0];
            string ruleName = sarifLog.Runs[0].Results[0].RuleId;

            if (result.RuleIndex > -1)
            {
                ruleName = sarifLog.Runs[0].Tool.Driver.Rules[result.RuleIndex].Name + ":" + ruleName;
            }

            metadata.Title = ruleName + 
                             ": Exposed credential(s) in '" + organization +
                             "/" + projectName + 
                             "' build definition: '" + 
                             buildDefinitionName + "'";

            metadata.AreaPath = areaPath;
            metadata.Description = "Default description";
            metadata.Tags = new List<string>(new string[] { "security" });

            return metadata;
        }

        public static string GetProjectName(this Uri projectUri)
        {
            string projectUriString = projectUri.OriginalString;
            int lastSlashIndex = projectUriString.LastIndexOf('/');

            string projectName = lastSlashIndex > 0 && lastSlashIndex < projectUriString.Length - 1
                ? projectUriString.Substring(lastSlashIndex + 1)
                : throw new ArgumentException($"Cannot parse project name from URI {projectUriString}");

            return projectName;
        }

        public static string GetAccountUriString(this Uri projectUri)
        {
            string projectUriString = projectUri.OriginalString;
            int lastSlashIndex = projectUriString.LastIndexOf('/');

            string accountUriString = projectUriString.Substring(0, lastSlashIndex);

            return accountUriString;
        }
    }
}
