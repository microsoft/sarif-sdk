// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
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
            string artifactId = artifactLocation.GetProperty<int>("ArtifactId").ToString();
            string projectName = artifactLocation.GetProperty("ProjectName");
            string artifactType = artifactLocation.GetProperty("ArtifactType");

            // BUG: GetProperty doesn't unencode string values
            string areaPath = $@"{workItemProjectName}" + artifactLocation.GetProperty("AreaPath").Replace(@"\\", @"\");

            string artifactName = artifactLocation.GetProperty("ArtifactName");

            List<string> customTags = new List<string>();
            if (artifactLocation.TryGetProperty("CustomTags", out string customTagsString))
            {
                metadata.CustomTags = customTagsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
            }

            Dictionary<string, string> customFields = new Dictionary<string, string>();
            if (artifactLocation.TryGetProperty("CustomFields", out string customFieldsString))
            {
                string[] fieldKvps = customFieldsString.Split(',');

                foreach (string fieldKvp in fieldKvps)
                {
                    string[] kv = fieldKvp.Split(':');

                    if (kv.Length == 2)
                    {
                        customFields.Add(kv[0].Trim(), kv[1].Trim());
                    }
                }
            }

            metadata.CustomFields = customFields;

            Result result = sarifLog.Runs[0].Results[0];
            string ruleName = sarifLog.Runs[0].Results[0].RuleId.Split('/')[0];

            if (result.RuleIndex > -1)
            {
                ruleName = sarifLog.Runs[0].Tool.Driver.Rules[result.RuleIndex].Name + ":" + ruleName;
            }

            string titleEntity;
            switch (artifactType)
            {
                case "builddefinitions":
                    titleEntity = "build definition";
                    break;
                case "releasedefinitions":
                    titleEntity = "release definition";
                    break;
                default:
                    titleEntity = string.Empty;
                    break;
            }

            metadata.Title = $"[{organization}/{projectName}] {ruleName}: Exposed credential(s) in {titleEntity}: '{artifactName}'";

            // TODO: This should come from the SARIF or command line arg in the future.
            metadata.Tags = new List<string>(new string[] { "Security" });
            metadata.AreaPath = areaPath;

            metadata.Description = InjectArguments(
                File.ReadAllText(templateFilePath),
                organization,
                projectName,
                artifactId,
                artifactName,
                ruleName,
                sarifLog.Runs[0]
                );

            return metadata;
        }

        private const string SCAN_LOCATIONS = "{{scanLocations}}";
        private const string SCAN_TARGET_LINK = "{{scanTargetLink}}";
        private const string SCAN_EXAMPLE_PLACEHOLDER = "{{scanExample}}";
        private const string SCAN_RULE_NAME_PLACEHOLDER = "{{scanRuleName}}";

        private static string InjectArguments(
            string templateText, 
            string organization, 
            string projectName, 
            string pipelineId,
            string artifactName,
            string ruleName, 
            Run run)
        {
            // Inject the rule name associated with the defect
            templateText = templateText.Replace(SCAN_RULE_NAME_PLACEHOLDER, ruleName);

            // Retrieve the rule descriptor
            List<ReportingDescriptor> rules = run.Tool.Driver.Rules as List<ReportingDescriptor>;
            ReportingDescriptor reportingDescriptor = rules.Where(r => r.Id == run.Results[0].GetRule(run).Id).FirstOrDefault();
            string exampleText = run.Results[0].GetMessageText(reportingDescriptor);
            exampleText = NormalizeExampleText(run.Results[0], exampleText);
            templateText = templateText.Replace(SCAN_EXAMPLE_PLACEHOLDER, exampleText);

            // Inject text from the first result
            Result sampleResult = run.Results[0];
            string anchorLink = GetLinkText(sampleResult.Message.Text);
            templateText = templateText.Replace(SCAN_TARGET_LINK, BuildAnchorElement(anchorLink, artifactName));

            string locationsText = BuildLocationsText(run);
            templateText = templateText.Replace(SCAN_LOCATIONS, locationsText);

            return templateText;
        }

        private static string NormalizeExampleText(Result result, string exampleText)
        {
            // [pipeline](https://dev.azure.com/msazure/One/_apps/hub/ms.vss-ciworkflow.build-ci-hub?_a=edit-build-definition&id=87109&view=Tab_Variables);

            if (!string.IsNullOrWhiteSpace(result.Locations?[0].LogicalLocations?[0]?.DecoratedName))
            {
                exampleText = exampleText.Replace(result.Locations?[0].LogicalLocations?[0]?.DecoratedName, result.Locations?[0].LogicalLocations?[0]?.FullyQualifiedName);
            }

            int pipelineIndex = exampleText.IndexOf("[pipeline]");
            int lastParenIndex = exampleText.LastIndexOf(')') + 1;

            string toReplace = exampleText.Substring(pipelineIndex, lastParenIndex - pipelineIndex);
            string link = toReplace.Substring("[pipeline]".Length + 1, toReplace.Length - "[pipeline]".Length - 2);
            string anchor = BuildAnchorElement(link, "pipeline");
            return exampleText.Replace(toReplace, anchor);
        }

        private static string BuildLocationsText(Run run)
        {
            IList<Result> results = run.Results;

            var sb = new StringBuilder();

            foreach (Result result in results)
            {
                ArtifactLocation artifactLocation = run.Artifacts[result.Locations[0].PhysicalLocation.ArtifactLocation.Index].Location;

                string organization = artifactLocation.GetProperty("OrganizationName");
                string artifactId = artifactLocation.GetProperty<int>("ArtifactId").ToString();
                string projectName = artifactLocation.GetProperty("ProjectName");
                string pipelineName = organization + "/" + projectName + "." + artifactId;

                string anchorLink = GetLinkText(result.Message.Text);
                string jsonPath = result.Locations?[0].LogicalLocations?[0]?.FullyQualifiedName ?? string.Empty;
                string anchorElement = BuildAnchorElement(anchorLink, jsonPath);
                sb.AppendLine($"{HttpUtility.HtmlEncode(anchorElement)}<br/>");
            }

            return sb.ToString();
        }

        private static string GetLinkText(string resultText)
        {
            // An apparent problem was found in 'jsonPath.variable' in this [pipeline](https://example.com).
            resultText = resultText.Split('(')[1];
            return resultText.Split(')')[0];
        }

        private static string BuildAnchorElement(string uri, string anchorText)
        {
            return $@"<a href=""{uri}"">{HttpUtility.HtmlEncode(anchorText)}</a>";
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
