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

        // A bare 'CWE-<number>' lacks only the required sub-id, so appending one repairs it
        // (the Missing path). Every other non-conformant shape is malformed beyond repair.
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

            // The gate is the convention emit hard-enforces via AIRuleIdConvention:
            // 'CWE-<number>/<kebab-sub-id>' or the flat 'NOVEL-<kebab-sub-id>' escape hatch.
            if (AIRuleIdConvention.IsAcceptable(ruleId))
            {
                return;
            }

            if (s_bareCweBaseId.IsMatch(ruleId))
            {
                // A GitHub-hosted run is the one place a bare 'CWE-<number>' is the expected,
                // correct shape: emit-finalize collapses each result's hierarchical ruleId to its
                // descriptor id for GitHub because GitHub's code-scanning security classifier binds
                // a result to its rule by ruleId-string equality with a reportingDescriptor.id and
                // does not honor SARIF's hierarchical-ruleId / ruleIndex resolution (SARIF §3.27.5,
                // §3.27.6). The sub-id requirement is suspended for that GitHub-only collapse and
                // still enforced everywhere else.
                if (VcpPortableRoot.IsGitHubHostedRun(run))
                {
                    return;
                }

                // The descriptor is consulted only to suggest a kebab-cased sub-id. A throw
                // here on a degenerate log (bad ruleIndex, absent driver) propagates to the
                // analysis engine's single rule-exception handler, which logs it.
                ReportingDescriptor rule = result.GetRule(run);

                // {1} is the bare ruleId; {2} is the suggested sub-id (kebab rule name or a slug).
                LogResult(
                    resultPointer,
                    nameof(RuleResources.AI1012_ProvideRuleSubId_Error_Missing_Text),
                    ruleId,
                    ToKebabCase(rule?.Name) ?? "readable-slug");
                return;
            }

            // Not a bare CWE base id and not acceptable: no appended sub-id can repair it.
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
