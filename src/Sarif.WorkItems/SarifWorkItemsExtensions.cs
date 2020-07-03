// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public static class SarifWorkItemsExtensions
    {
        public static bool ShouldBeFiled(this Result result)
        {
            if (result.BaselineState == BaselineState.Absent ||
                result.BaselineState == BaselineState.Updated)
            {
                return false;
            }
                        
            if (result.Suppressions?.Count > 0) { return false; }

            // Fail: an explicit failure occurred.
            //
            // Open: a sound analysis is 'open' indicating that the analysis requires
            //       more configuration or other data in order to produce a determination.
            // 
            // Review: an open item which can't be automated, i.e., which requires a
            //         manual review to resolve.
            return result.Kind == ResultKind.Fail ||
                   result.Kind == ResultKind.Open ||
                   result.Kind == ResultKind.Review;

            // Designations which are not appropriate for filing:
            //
            // Pass: the scan target explicitly passed analysis.
            // 
            // Not applicable: analysis was skipped because it was not
            //                 relevant to the scan target.
            //
            // Informational: a strictly informational message was emitted.
        }

        public static string GetFingerprintSplittingStrategyId(this Result result)
        {
            return $"{result.GetProperty("OrganizationName")}:{result.GetProperty("EtlEntity")}:{result.PartialFingerprints["SecretHash/v1"]}";
        }

        public static string GetPerRepositoryFingerprintSplittingStrategyId(this Result result)
        {
            string projectId = string.Empty;
            result.TryGetProperty("ProjectId", out projectId);

            string repositoryId = string.Empty;
            result.TryGetProperty("RepositoryId", out repositoryId);

            return $"{result.GetProperty("OrganizationName")}:{projectId ?? string.Empty}:{result.GetProperty("EtlEntity")}:{repositoryId ?? string.Empty}:{result.PartialFingerprints["SecretHash/v1"]}";
        }

        public static string CreateWorkItemTitle(this Run run)
        {
            if (run == null) { throw new ArgumentNullException(nameof(run)); }
            if (run.Results == null) { throw new ArgumentNullException(nameof(run.Results)); }

            Result firstResult = null;

            foreach (Result result in run?.Results)
            {
                if (result.ShouldBeFiled())
                {
                    firstResult = result;
                    break;
                }
            }

            // No useful work to schedule in a work item, apparently.
            if (firstResult == null) { return null; }

            string fullRuleId = ConstructFullRuleIdentifier(firstResult.GetRule(run));

            string titlePrefix = "[" + run.Tool.Driver.Name + ":" +
                    firstResult.Level.ToString() + "]: " +
                    fullRuleId;

            // In ADO, the title cannot be longer than 256 characters
            const string ellipsis = "...";
            const int maxChars = 256;
            int remainingChars = maxChars - titlePrefix.Length - 6; // " ({0})".Length == 6

            // We encapsulate logical names in apostrophes to help indicate they are a symbol
            string locationName = "'" + firstResult.Locations?[0].LogicalLocation?.FullyQualifiedName + "'";

            if (locationName.Length > remainingChars)
            { 
                locationName = "'" + Path.GetFileName(firstResult.Locations?[0].LogicalLocation?.FullyQualifiedName) + "'";

                if (locationName.Length > remainingChars)
                {
                    locationName = "'" + firstResult.Locations?[0].LogicalLocation?.FullyQualifiedName.Substring(0, remainingChars - ellipsis.Length - 2) + ellipsis + "'";
                }
            }

            if (locationName.Equals("''"))
            {
                // We don't bother to wrap a file path or URL in apostrophes as these are self-evident
                //
                // Lines of code like this that inspire strong feelings in SARIF consumers as far as its design.
                locationName = firstResult.Locations?[0].PhysicalLocation?.ArtifactLocation?.Resolve(run)?.Uri?.OriginalString;

                if (locationName?.Length > remainingChars)
                {
                    locationName = locationName.Substring(0, remainingChars - ellipsis.Length) + ellipsis;
                }
            }

            // Returns strings like:
            //
            // [Tool:Warning] RULE3067 (in 'Namespace.Type.MyMethod()')
            // [Tool:Error] RULE2001: Null dereference (in c:\src\build.cpp)

            string title = titlePrefix + (locationName == null ? "" : " (in " + locationName + ")");

            return title;
        }

        public static List<string> GetToolNames(this SarifLog log)
        {
            return log?.Runs?
                .Where(r => r?.Results?.Count > 0)
                .Select(r => r.Tool?.Driver?.Name)
                .Distinct()
                .ToList();
        }

        public static int GetAggregateFilableResultsCount(this SarifLog log)
        {
            return log?.Runs?
                .Select(run => run?.Results?
                    .Select(result => result.ShouldBeFiled() ? 1 : 0).Sum())
                .Sum() ?? 0;
        }
        
        public static string CreateWorkItemDescription(this SarifLog log, SarifWorkItemContext context, IList<Uri> locationUris)
        {
            int totalResults = log.GetAggregateFilableResultsCount();
            List<string> toolNames = log.GetToolNames();
            string phrasedToolNames = toolNames.ToAndPhrase();
            string multipleToolsFooter = toolNames.Count > 1 ? WorkItemsResources.MultipleToolsFooter : string.Empty;

            Uri runRepositoryUri = log?.Runs.FirstOrDefault()?.VersionControlProvenance?.FirstOrDefault().RepositoryUri;
            Uri detectionLocationUri = !string.IsNullOrEmpty(runRepositoryUri?.OriginalString) ? runRepositoryUri : locationUris?[0];

            string detectionLocation = (detectionLocationUri?.IsAbsoluteUri == true && detectionLocationUri?.Scheme == "https")
                ? context.CreateLinkText(detectionLocationUri.OriginalString, detectionLocationUri?.OriginalString)
                : detectionLocationUri?.OriginalString;

            if (locationUris?.Count > 1)
            {
                int additionalLocations = locationUris.Count - 1;
                detectionLocation = $"{detectionLocation} (+{additionalLocations} locations)";
            }

            // This work item contains {0} {1} issue(s) detected in {2}{3}. Click the 'Scans' tab to review results.
            string description =
                string.Format(
                    WorkItemsResources.WorkItemBodyTemplateText, // This work item contains
                    totalResults,                                // {0}
                    phrasedToolNames,                            // {1} issue(s) detected in
                    detectionLocation,                           // {2}
                    multipleToolsFooter);                        // {3}. Click the 'Scans' tab to review results.\r\n

            return description;
        }

        private static string ConstructFullRuleIdentifier(ReportingDescriptor reportingDescriptor)
        {
            string fullRuleIdentifier = reportingDescriptor.Id;

            if (!string.IsNullOrEmpty(reportingDescriptor.Name))
            {
                fullRuleIdentifier += ": " + reportingDescriptor.Name;
            }
            return fullRuleIdentifier;
        }
    }
}
