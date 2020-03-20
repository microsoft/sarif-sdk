// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.TeamFoundation.TestManagement.WebApi.Legacy;
using Octokit;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public static class SarifWorkItemsExtensions
    {
        public static bool AppropriateForFiling(this Result result)
        {
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
                if (result.AppropriateForFiling())
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

        public static Dictionary<string, int> ComputeToolResultCounts(this SarifLog log)
        {
            if (log == null) { throw new ArgumentNullException(nameof(log)); }
            if (log.Runs == null) { throw new ArgumentNullException(nameof(log.Runs)); }

            var resultCountsByTool = new Dictionary<string, int>();

            foreach (Run run in log?.Runs)
            {
                if (run != null && run.Results != null)
                {
                    resultCountsByTool.Add(run?.Tool?.Driver?.Name, run.Results.Count);
                }
            }

            return resultCountsByTool;
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

        public static string CreateWorkItemDescription(this SarifLog log)
        {
            Dictionary<string, int> resultCountsByTool = ComputeToolResultCounts(log);
            StringBuilder templateText = new StringBuilder(@"This bug has been filed by the Sarif Work Item Automatic Filer.  It contains results for the following tools and issues:");
            templateText.AppendLine();
            templateText.AppendLine();

            foreach (KeyValuePair<string, int> toolResult in resultCountsByTool)
            {
                templateText.Append(string.Format("*Tool: {0}", toolResult.Key));
                templateText.AppendLine();
                templateText.Append(string.Format("     Result count: {0}", toolResult.Value));
                templateText.AppendLine();
                templateText.AppendLine();
            }

            templateText.AppendLine();
            templateText.Append(@"To see result details, please visit the Scans tab of this bug, or the attached Sarif log.");
            templateText.Append(@"If the scans tab is missing or unavailable, please install the Sarif viewer from https://marketplace.visualstudio.com/items?itemName=WDGIS.MicrosoftSarifViewer");

            return templateText.ToString();
        }
    }
}
