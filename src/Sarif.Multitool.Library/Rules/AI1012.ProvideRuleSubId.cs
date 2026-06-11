// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.Sarif.Emit;

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
            nameof(RuleResources.AI1012_ProvideRuleSubId_Error_Malformed_Text)
        };

        // A bare CWE base id (CWE-<number>) is conformant except that it lacks the required
        // sub-id; appending one (e.g. CWE-89/kql-injection) makes it acceptable. Every other
        // non-conformant shape is malformed: it can never be repaired by appending a sub-id.
        private static readonly Regex s_bareCweBaseId = new Regex(
            @"^CWE-[0-9]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        protected override void Analyze(Result result, string resultPointer)
        {
            Run run = Context.CurrentRun;
            string ruleId = result.ResolvedRuleId(run);
            if (string.IsNullOrEmpty(ruleId))
            {
                // SARIF1010 / other rules cover the missing-ruleId case.
                return;
            }

            // The validation gate is the same convention emit hard-enforces via
            // AIRuleIdConvention.ThrowIfAnyUnacceptable: result.ruleId is either
            // 'CWE-<number>/<kebab-sub-id>' or the flat 'NOVEL-<kebab-sub-id>' escape hatch.
            if (AIRuleIdConvention.IsAcceptable(ruleId))
            {
                return;
            }

            if (s_bareCweBaseId.IsMatch(ruleId))
            {
                // GetRule can NRE on degenerate logs (e.g. tool.driver absent, extension-only
                // tool, bad ruleIndex). A validation skimmer must never throw on malformed
                // input — those shapes are other rules' job to flag. The descriptor is consulted
                // only to suggest a kebab-cased sub-id; failure to resolve it is non-fatal.
                ReportingDescriptor rule = null;
                try { rule = result.GetRule(run); } catch { /* fall through with rule == null */ }

                // {0}: 'result.ruleId' is '{1}' with no sub-component beyond the descriptor id.
                // Append a hierarchical sub-ID: a true sub-classification of '{1}' if one applies,
                // otherwise the kebab-cased rule name (e.g. '{1}/{2}').
                LogResult(
                    resultPointer,
                    nameof(RuleResources.AI1012_ProvideRuleSubId_Error_Missing_Text),
                    ruleId,
                    ToKebabCase(rule?.Name) ?? "readable-slug");
                return;
            }

            // {0}: 'result.ruleId' is '{1}', which is neither a 'CWE-<number>/<sub-id>' nor a
            // 'NOVEL-<sub-id>' value, so no appended sub-id can make it conform.
            LogResult(
                resultPointer,
                nameof(RuleResources.AI1012_ProvideRuleSubId_Error_Malformed_Text),
                ruleId);
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
