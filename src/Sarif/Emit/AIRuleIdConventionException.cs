// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Thrown by the AI-authoring emit chain when one or more <see cref="Result.RuleId"/>
    /// values violate <see cref="AIRuleIdConvention"/>.
    /// </summary>
    /// <remarks>
    /// The exception message lists every offending id, the accepted shapes, and the
    /// documentation pointer. <see cref="OffendingRuleIds"/> exposes the same ids for
    /// programmatic consumers.
    /// </remarks>
    public sealed class AIRuleIdConventionException : Exception
    {
        /// <summary>
        /// Stable error code so downstream tooling can pattern-match without parsing the
        /// human-readable message body.
        /// </summary>
        public const string ErrorCode = "AI-RULEID-001";

        public AIRuleIdConventionException(IList<string> offendingRuleIds)
            : base(BuildMessage(offendingRuleIds))
        {
            OffendingRuleIds = new ReadOnlyCollection<string>(
                offendingRuleIds ?? Array.Empty<string>());
        }

        /// <summary>
        /// The rejected <see cref="Result.RuleId"/> values, in source order. An empty string
        /// in this list represents a result that supplied no ruleId at all.
        /// </summary>
        public IReadOnlyList<string> OffendingRuleIds { get; }

        private static string BuildMessage(IList<string> offendingRuleIds)
        {
            var sb = new StringBuilder();
            sb.Append("error ").Append(ErrorCode).Append(": ");
            int count = offendingRuleIds?.Count ?? 0;
            if (count == 0)
            {
                sb.Append("result.ruleId does not conform to the AI ruleId convention.");
            }
            else
            {
                sb.Append(count).Append(" result");
                if (count != 1) { sb.Append('s'); }
                sb.AppendLine(" did not conform to the AI ruleId convention:");
                for (int i = 0; i < count; i++)
                {
                    string offender = offendingRuleIds[i];
                    sb.Append("  - ");
                    if (string.IsNullOrEmpty(offender))
                    {
                        sb.AppendLine("(empty ruleId)");
                    }
                    else
                    {
                        sb.Append('\'').Append(offender).AppendLine("'");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("Every AI-emitted result.ruleId MUST take one of two shapes:");
            sb.AppendLine("  1. Taxonomy sub-id  CWE-<number>/<sub-id>");
            sb.AppendLine("     e.g., 'CWE-89/kql-injection-from-config'");
            sb.AppendLine("     Use this whenever the finding maps to a CWE entry.");
            sb.AppendLine("     The base id (CWE-89) drives descriptor enrichment; the sub-id");
            sb.AppendLine("     is your AI-chosen sub-classifier and keeps AI1012 silent.");
            sb.AppendLine("  2. NOVEL escape hatch  NOVEL-<sub-id>");
            sb.AppendLine("     e.g., 'NOVEL-prompt-injection-via-system-message'");
            sb.AppendLine("     Use this ONLY when no CWE entry fits. The NOVEL- form is");
            sb.AppendLine("     flat (no slash). If the finding maps to a CWE entry,");
            sb.AppendLine("     use shape #1 instead.");
            sb.AppendLine();
            sb.Append("Retry the emit after correcting every offender above. ");
            sb.Append("See docs/AI-RuleId-Convention.md for full guidance.");

            return sb.ToString();
        }
    }
}
