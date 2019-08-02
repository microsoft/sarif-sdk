// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public static class Extensions
    {
        public static WorkItemFilingMetadata CreateWorkItemFilingMetadata(this SarifLog sarifLog, string workItemProjectName, string templateFilePath)
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

            string organization = artifactLocation.GetProperty("OrganizationName");
            string buildDefinitionId = artifactLocation.GetProperty<int>("BuildDefinitionId").ToString();
            string projectName = artifactLocation.GetProperty("ProjectName");

            // BUG: GetProperty doesn't unencode string values
            string areaPath = $@"{workItemProjectName}" + artifactLocation.GetProperty("AreaPath").Replace(@"\\", @"\");

            string buildDefinitionName = artifactLocation.GetProperty("BuildDefinitionName");

            Result result = sarifLog.Runs[0].Results[0];
            string ruleName = sarifLog.Runs[0].Results[0].RuleId;

            if (result.RuleIndex > -1)
            {
                ruleName = sarifLog.Runs[0].Tool.Driver.Rules[result.RuleIndex].Name + ":" + ruleName;
            }

            metadata.Title = "[" + organization + "/" + projectName + "] " +
                            ruleName + ": Exposed credential(s) in " +
                             "build definition: '" + buildDefinitionName + "'";

            metadata.Tags = new List<string>(new string[] { "security" });
            metadata.AreaPath = areaPath;

            metadata.Description = InjectArguments(
                File.ReadAllText(templateFilePath),
                organization,
                projectName,
                buildDefinitionName,
                ruleName,
                sarifLog.Runs[0]
                );

            return metadata;
        }

        private const string SCAN_LOCATIONS = "{{scanLocations}}";
        private const string SCAN_TARGET_LINK = "{{scanTargetLink}}";
        private const string SCAN_EXAMPLE_PLACEHOLDER = "{{scanExample}}";
        private const string SCAN_RULE_NAME_PLACEHOLDER = "{{scanRuleName}}";

        private static string InjectArguments(string templateText, string organization, string projectName, string buildDefinitionName, string ruleName, Run run)
        {
            // Inject the rule name associated with the defect
            templateText = templateText.Replace(SCAN_RULE_NAME_PLACEHOLDER, ruleName);

            // Retrieve the rule descriptor
            List<ReportingDescriptor> rules = run.Tool.Driver.Rules as List<ReportingDescriptor>;
            ReportingDescriptor reportingDescriptor = rules.Where(r => r.Id == run.Results[0].GetRule(run).Id).FirstOrDefault();
            templateText = templateText.Replace(SCAN_EXAMPLE_PLACEHOLDER, run.Results[0].GetMessageText(reportingDescriptor));

            // Inject text from the first result
            Result sampleResult = run.Results[0];
            string anchorLink = GetLinkText(sampleResult.Message.Text);
            templateText = templateText.Replace(SCAN_TARGET_LINK, BuildAnchorElement(anchorLink, buildDefinitionName));

            string jsonPath = GetVariableName(sampleResult.Message.Text);
            string locationsText = BuildLocationsText(run.Results);
            templateText = templateText.Replace(SCAN_LOCATIONS, locationsText);

            return templateText;
        }

        private static string BuildLocationsText(IList<Result> results)
        {
            var sb = new StringBuilder();

            foreach (Result result in results)
            {
                string anchorLink = GetLinkText(result.Message.Text);
                string jsonPath = GetVariableName(result.Message.Text);
                string anchorElement = BuildAnchorElement(anchorLink, jsonPath);
                sb.AppendLine(anchorElement + "<br/>");
            }

            return sb.ToString();
        }

        private static string GetVariableName(string resultText)
        {
            // An apparent problem was found in 'jsonPath.variable' in this [pipeline](https://example.com).

            int firstIndex = resultText.IndexOf('\'');
            int lastIndex = resultText.LastIndexOf('\'');
            return resultText.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
        }

        private static string GetLinkText(string resultText)
        {
            // An apparent problem was found in 'jsonPath.variable' in this [pipeline](https://example.com).
            resultText = resultText.Split('(')[1];
            return resultText.Split(')')[0];
        }

        private static string BuildAnchorElement(string uri, string anchorText)
        {
            return $@"<a href=""{uri}"">{anchorText}</a>";
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
