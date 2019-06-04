// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Result
    {
        public bool ShouldSerializeWorkItemUris() { return this.WorkItemUris != null && this.WorkItemUris.Any((s) => s != null); }

        public bool ShouldSerializeLevel() { return this.Level != FailureLevel.Warning; }

        /// <summary>
        ///  Look up the ReportingDescriptor for this Result.
        /// </summary>
        /// <param name="run">Run instance containing this Result</param>
        /// <returns>ReportingDescriptor for Result Rule, if available</returns>
        public ReportingDescriptor GetReportingDescriptor(Run run)
        {
            // Follows SARIF Spec 3.52.3 (reportingDescriptor lookup)

            // Find the 'ToolComponent' for this Result (Run.Tool.Driver if absent)
            ToolComponent component = run.GetToolComponentFromReference(this.Rule?.ToolComponent);
            IList<ReportingDescriptor> rules = component?.Rules;

            // Look up by this.RuleIndex, if present
            if (this.RuleIndex >= 0)
            {
                return GetRuleByIndex(rules, this.RuleIndex);
            }

            // Look up by this.RuleDescriptor.Index, if present
            if (this.Rule?.Index >= 0)
            {
                return GetRuleByIndex(rules, this.Rule.Index);
            }

            // Look up by this.RuleDescriptor, Guid, if present
            if (!String.IsNullOrEmpty(this.Rule?.Guid))
            {
                if (rules != null)
                {
                    foreach (ReportingDescriptor rule in rules)
                    {
                        if (rule.Guid == this.Rule.Guid) { return rule; }
                    }

                    throw new ArgumentException($"ReportingDescriptorReference referred to Guid {this.Rule.Guid}, which was not found in toolComponent.Rules.");
                }
            }

            // Otherwise, metadata is not available and RuleId is the only available property
            return new ReportingDescriptor() { Id = this.RuleId ?? this.Rule?.Id };
        }

        private static ReportingDescriptor GetRuleByIndex(IList<ReportingDescriptor> rules, int ruleIndex)
        {
            if (rules == null)
            {
                throw new ArgumentException("ToolComponent referred to by Result has no Rules collection.");
            }

            if (ruleIndex < 0 || ruleIndex >= rules.Count)
            {
                throw new ArgumentOutOfRangeException($"Result refers to rule index {ruleIndex}, but ToolComponent.Rules has only {rules.Count} rules.");
            }

            return rules[ruleIndex];
        }
    }
}
