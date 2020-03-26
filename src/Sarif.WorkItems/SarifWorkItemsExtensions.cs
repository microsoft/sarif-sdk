// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public static class SarifWorkItemsExtensions
    {
        public static bool ShouldBeFiled(this Result result)
        {
            if (result.BaselineState != BaselineState.None &&
                result.BaselineState != BaselineState.New) 
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

            // We encapsulate logical names in apostrophes to help indicate they are a symbol
            string locationName = "'" + firstResult.Locations?[0].LogicalLocation?.FullyQualifiedName + "'";

            if (locationName.Equals("''"))
            {
                // We don't bother to wrap a file path or URL in apostrophes as these are self-evident
                //
                // Lines of code like this that inspire strong feelings in SARIF consumers as far as its design.
                locationName = firstResult.Locations?[0].PhysicalLocation?.ArtifactLocation?.Resolve(run)?.Uri?.OriginalString;
            }

            // Returns strings like:
            //
            // [Tool:Warning] RULE3067 (in 'Namespace.Type.MyMethod()')
            // [Tool:Error] RULE2001: Null derefernece (in c:\src\build.cpp)

            return "[" + run.Tool.Driver.Name + ":" +
                    firstResult.Level.ToString() + "]: " +
                    fullRuleId +
                    (locationName == null ? "" : " (in " + locationName + ")");
        }

        public static List<string> GetRunToolNames(this SarifLog log)
        {
            return log?.Runs?.Where(r => r?.Results?.Count > 0).Select(r => r.Tool?.Driver?.Name).Distinct().ToList();
        }

        public static int GetAggregateResultCount(this SarifLog log)
        {
            return log?.Runs?.Select(r => r?.Results?.Count).Sum() ?? 0;
        }

        public static string PhraseToolNames(List<string> toolNames)
        {
            toolNames = toolNames.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
            if (toolNames.Count == 0)
            {
                return string.Empty;
            }
            else if (toolNames.Count == 1)
            {
                return string.Format("'{0}'", toolNames[0]);
            }
            else
            {
                string phrasedToolNames = string.Format("'{0}'", toolNames[0]);
                for (int j = 1; j < toolNames.Count - 1; j++)
                {
                    phrasedToolNames = string.Join(", ", phrasedToolNames, string.Format("'{0}'", toolNames[j]));
                }
                phrasedToolNames = string.Join(" and ", phrasedToolNames, string.Format("'{0}'", toolNames[toolNames.Count - 1]));
                return phrasedToolNames;
            }
        }

        public static string CreateWorkItemDescription(this SarifLog log, Uri locationUri)
        {
            List<string> toolNames = log.GetRunToolNames();
            int totalResults = log.GetAggregateResultCount();
            string phrasedToolNames = PhraseToolNames(toolNames);
            string multipleToolsFooter = toolNames.Count > 1 ? WorkItemsResources.MultipleToolsFooter : string.Empty;

            Uri runRepositoryUri = log?.Runs.FirstOrDefault()?.VersionControlProvenance?.FirstOrDefault().RepositoryUri;
            string detectionLocation = !string.IsNullOrEmpty(runRepositoryUri?.OriginalString) ? runRepositoryUri?.OriginalString : locationUri?.OriginalString;
          
            var descriptionText = new StringBuilder(string.Format(WorkItemsResources.WorkItemBodyTemplateText, totalResults, phrasedToolNames, detectionLocation, multipleToolsFooter));
            descriptionText.Append(WorkItemsResources.ViewScansTabResults);
            descriptionText.AppendLine();

            return descriptionText.ToString();
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
