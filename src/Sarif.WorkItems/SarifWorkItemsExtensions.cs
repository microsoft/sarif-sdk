// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

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
            Result firstResult = null;
            var ruleIds = new HashSet<string>();

            foreach (Result result in run?.Results)
            {
                if (!result.AppropriateForFiling())
                {                     
                    continue; 
                }

                string ruleId = result.ResolvedRuleId(run);
                firstResult = firstResult ?? result;
                ruleIds.Add(ruleId);
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
                locationName = firstResult.Locations?[0].PhysicalLocation?.ArtifactLocation?.Resolve(run)?.Uri.OriginalString;
            }

            // Returns strings like:
            //
            // [Tool:Warning] RULE3067 (in 'Namespace.Type.MyMethod()')
            // [Tool:Error] RULE2001: Null derefernece (in c:\src\build.cpp)

            return "[" + run.Tool.Driver.Name + ":" +
                    firstResult.Level.ToString() + "]: " +
                    fullRuleId +
                    (locationName == null ? "" : " (in file " + locationName + ")");
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
