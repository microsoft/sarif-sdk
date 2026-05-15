// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideRuleSubId : SarifValidationSkimmerBase
    {
        public ProvideRuleSubId()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1012
        /// </summary>
        public override string Id => RuleId.AIProvideRuleSubId;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1012_ProvideRuleSubId_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1012_ProvideRuleSubId_Error_Missing_Text),
            nameof(RuleResources.AI1012_ProvideRuleSubId_Error_DescriptorIdContainsSlash_Text)
        };

        // Walk the descriptors *first* (Skimmer's tree walk doesn't guarantee descriptor-before-
        // result, and we want the descriptor-level violation reported even when no result references
        // that descriptor). The descriptor-id-contains-slash anti-pattern is detected here; the
        // result-must-have-sub-id check happens in Analyze(Result, ...).
        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (string.IsNullOrEmpty(reportingDescriptor?.Id))
            {
                // Other rules cover missing descriptor IDs; not our concern.
                return;
            }

            if (reportingDescriptor.Id.IndexOf('/') >= 0)
            {
                // Anti-pattern: the descriptor's stable identifier carries a hierarchical
                // sub-component (e.g. 'CWE-78/api-handler'). Per SARIF §3.49.3 NOTE 2 / §3.52.4 the
                // sub-component belongs on result.ruleId only — descriptor.id must be the base ID
                // ('CWE-78') so that descriptor identity is stable across results and the
                // tool.driver.rules array doesn't explode with one entry per result.
                LogResult(
                    reportingDescriptorPointer.AtProperty(SarifPropertyName.Id),
                    nameof(RuleResources.AI1012_ProvideRuleSubId_Error_DescriptorIdContainsSlash_Text),
                    reportingDescriptor.Id);
            }
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            Run run = Context.CurrentRun;
            string ruleId = result.ResolvedRuleId(run);
            if (string.IsNullOrEmpty(ruleId))
            {
                // SARIF1010 / other rules cover the missing-ruleId case.
                return;
            }

            // GetRule can NRE on degenerate logs (e.g. tool.driver absent, extension-only
            // tool, bad ruleIndex). A validation skimmer must never throw on malformed
            // input — those shapes are other rules' job to flag.
            ReportingDescriptor rule = null;
            try { rule = result.GetRule(run); } catch { /* fall through with rule == null */ }
            string descriptorId = rule?.Id;

            // SARIF §3.27.5 / §3.49.3 NOTE 2 / §3.52.4: the AI profile requires every result to
            // carry a hierarchical sub-component on result.ruleId BEYOND the descriptor's base id.
            // We do NOT accept "descriptor.id already contains a slash" — that anti-pattern is
            // flagged separately by the descriptor-side Analyze override above. The valid shape
            // is descriptor.id="CWE-78", result.ruleId="CWE-78/api-handler".
            bool hasSubIdOnResult = !string.IsNullOrEmpty(descriptorId)
                ? ruleId.Length > descriptorId.Length
                    && ruleId.StartsWith(descriptorId, System.StringComparison.Ordinal)
                    && ruleId[descriptorId.Length] == '/'
                : ruleId.IndexOf('/') >= 0;

            if (hasSubIdOnResult)
            {
                return;
            }

            // {0}: 'result.ruleId' is '{1}' with no sub-component beyond the descriptor id.
            // Append a hierarchical sub-ID: a true sub-classification of '{1}' if one applies,
            // otherwise the kebab-cased rule name (e.g. '{1}/{2}').
            LogResult(
                resultPointer,
                nameof(RuleResources.AI1012_ProvideRuleSubId_Error_Missing_Text),
                ruleId,
                ToKebabCase(rule?.Name) ?? "readable-slug");
        }

        // PlanEventMissingAuthorization -> plan-event-missing-authorization
        internal static string ToKebabCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var sb = new StringBuilder(name.Length + 8);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(c))
                {
                    if (sb.Length > 0 && sb[sb.Length - 1] != '-')
                    {
                        sb.Append('-');
                    }
                    sb.Append(char.ToLowerInvariant(c));
                }
                else if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else if (sb.Length > 0 && sb[sb.Length - 1] != '-')
                {
                    sb.Append('-');
                }
            }

            return sb.Length > 0 ? sb.ToString().Trim('-') : null;
        }
    }
}
