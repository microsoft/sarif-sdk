// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideRuleSubId : SarifValidationSkimmerBase
    {
        public ProvideRuleSubId()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
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
            nameof(RuleResources.AI1012_ProvideRuleSubId_Warning_Missing_Text)
        };

        private Run run;

        protected override void Analyze(Run run, string runPointer)
        {
            this.run = run;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            string ruleId = result.ResolvedRuleId(this.run);
            if (string.IsNullOrEmpty(ruleId))
            {
                // SARIF1010 / other rules cover the missing-ruleId case.
                return;
            }

            // GetRule can NRE on degenerate logs (e.g. tool.driver absent, extension-only
            // tool, bad ruleIndex). A validation skimmer must never throw on malformed
            // input — those shapes are other rules' job to flag.
            ReportingDescriptor rule = null;
            try { rule = result.GetRule(this.run); } catch { /* fall through with rule == null */ }
            string descriptorId = rule?.Id;

            // The sub-ID requirement: result.ruleId must have at least one hierarchical component
            // beyond the descriptor's base id. If we can't resolve a descriptor, fall back to
            // checking for any '/' at all.
            bool hasSubId = !string.IsNullOrEmpty(descriptorId)
                ? ruleId.Length > descriptorId.Length
                  && ruleId.StartsWith(descriptorId, System.StringComparison.Ordinal)
                  && ruleId[descriptorId.Length] == '/'
                : ruleId.IndexOf('/') >= 0;

            if (hasSubId)
            {
                return;
            }

            // {0}: 'result.ruleId' is '{1}' with no sub-component beyond the descriptor id.
            // Append a hierarchical sub-ID: a true sub-classification of '{1}' if one applies,
            // otherwise the kebab-cased rule name (e.g. '{1}/{2}').
            LogResult(
                resultPointer,
                nameof(RuleResources.AI1012_ProvideRuleSubId_Warning_Missing_Text),
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
