// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Result
    {
        /// <summary>
        ///  Reference to the Run containing this Result, if available.
        /// </summary>
        /// <remarks>
        ///  Used to look up Result details which may be on Run collections (ex: Run.Tool.Driver.Rules)
        /// </remarks>
        public Run Run { get; set; }

        public bool ShouldSerializeWorkItemUris() { return this.WorkItemUris != null && this.WorkItemUris.Any((s) => s != null); }

        public bool ShouldSerializeLevel() { return this.Level != FailureLevel.Warning; }

        public void EnsureRunProvided()
        {
            if (this.Run == null)
            {
                throw new ArgumentException($"Result.Run was required but not provided. Ensure Result.Run properties are populated by calling Run.SetRunOnResults().");
            }
        }

        /// <summary>
        ///  Resolve the RuleId for this Result, from direct properties or via Run.Rules lookup.
        /// </summary>
        /// <param name="run">Run containing this Result</param>
        /// <returns>RuleId of this Result</returns>
        public string ResolvedRuleId(Run run)
        {
            return RuleId ?? Rule?.Id ?? GetRule(run ?? this.Run)?.Id;
        }

        /// <summary>
        ///  Look up the ReportingDescriptor for this Result.
        /// </summary>
        /// <param name="run">Run instance containing this Result</param>
        /// <returns>ReportingDescriptor for Result Rule, if available</returns>
        public ReportingDescriptor GetRule(Run run = null)
        {
            // Follows SARIF Spec 3.52.3 (reportingDescriptor lookup)

            // Ensure run argument or Result.Run was set
            if (run == null)
            {
                EnsureRunProvided();
                run = this.Run;
            }

            if (run != null)
            {
                // Find the 'ToolComponent' for this Result (Run.Tool.Driver if absent)
                ToolComponent component = run.GetToolComponentFromReference(this.Rule?.ToolComponent);
                IList<ReportingDescriptor> rules = component?.Rules;

                // Look up by this.RuleIndex, if present
                if (this.RuleIndex >= 0)
                {
                    return GetRuleByIndex(rules, this.RuleIndex);
                }

                // Look up by this.Rule.Index, if present
                if (this.Rule?.Index >= 0)
                {
                    return GetRuleByIndex(rules, this.Rule.Index);
                }

                // Look up by this.Rule.Guid, if present
                if (!string.IsNullOrEmpty(this.Rule?.Guid) && rules != null)
                {
                    ReportingDescriptor rule = component.GetRuleByGuid(this.Rule.Guid);
                    if (rule != null) { return rule; }
                }

                // Look up by this.RuleId or this.Rule.Id, if present
                string ruleId = this.RuleId ?? this.Rule?.Id;
                if (ruleId != null && rules != null)
                {
                    ReportingDescriptor rule = component.GetRuleById(ruleId);
                    if (rule != null) { return rule; }
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

#if DEBUG
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();

            sb.Append(this.Locations?[0].PhysicalLocation?.ArtifactLocation?.Uri);
            sb.Append(" : " + this.RuleId);
            sb.Append(" : " + this.Level);
            sb.Append(" : " + this.Kind);

            if (!string.IsNullOrEmpty(this.Message?.Text))
            {
                sb.Append(" : " + this.Message.Text);
            }
            else if (this.Message?.Arguments != null)
            {
                sb.Append(" : {");
                foreach (string argument in this.Message.Arguments)
                {
                    sb.Append(argument + ",");
                }
                sb.Length -= 1;
                sb.Append("}");
            }

            return sb.ToString();
        }
#endif
    }
}
