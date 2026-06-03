// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Enforces the SARIF SDK AI-authoring convention for <see cref="Result.RuleId"/>.
    /// </summary>
    /// <remarks>
    /// <para>The emit verb chain (and any future AI-facing acceptor on top of the same SDK)
    /// is opinionated about what a well-shaped AI finding's <see cref="Result.RuleId"/>
    /// looks like. Every accepted result MUST carry a ruleId in one of two forms:</para>
    /// <list type="bullet">
    /// <item><description><b>CWE sub-id</b> — <c>CWE-&lt;number&gt;/&lt;sub-id&gt;</c> where
    /// <c>sub-id</c> is a non-empty AI-chosen sub-classifier in lowercase alphanumeric
    /// kebab-case (single hyphens, no leading/trailing/consecutive hyphen),
    /// e.g., <c>CWE-89/kql-injection-from-config</c>. AI findings are always CWE-based;
    /// other taxonomies (CVE, OWASP) are not accepted on this path.</description></item>
    /// <item><description><b>NOVEL escape hatch</b> — <c>NOVEL-&lt;sub-id&gt;</c> for
    /// findings that don't map to any CWE entry
    /// (e.g., <c>NOVEL-prompt-injection-via-system-message</c>). The NOVEL- form is
    /// exclusive: it does not accept a slash. If the AI can connect the finding back to
    /// a CWE entry it MUST use the sub-id form instead.</description></item>
    /// </list>
    /// <para>Rationale: the sub-id form keeps AI1012 silent (sub-classification is what
    /// the rule wants) AND lets the CWE taxonomy enricher hydrate the base descriptor
    /// from MITRE metadata, so the AI gets enriched output for free while staying
    /// honest about which sub-pattern of the base it observed. The NOVEL- form keeps
    /// non-CWE findings emittable without forcing the AI to pretend a CWE applies.
    /// See <c>docs/AI-RuleId-Convention.md</c> for the full rationale and examples.</para>
    /// <para>Producers using <see cref="Writers.SarifLogger"/> directly do not flow through
    /// this convention — it is specific to the AI-authoring emit verb path.</para>
    /// </remarks>
    public static class AIRuleIdConvention
    {
        // CWE sub-id form: the literal CWE- prefix with a numeric base id, then a slash,
        // then a non-empty lowercase-alphanumeric kebab sub-id (single hyphens, no
        // leading/trailing/consecutive hyphen). AI findings are always CWE-based.
        private static readonly Regex s_taxonomySubId = new Regex(
            @"^CWE-[0-9]+/[a-z0-9]+(-[a-z0-9]+)*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // NOVEL escape hatch: "NOVEL-" followed by a lowercase-alphanumeric kebab sub-id
        // (single hyphens, no leading/trailing/consecutive hyphen). No slashes, no whitespace.
        private static readonly Regex s_novelPrefix = new Regex(
            @"^NOVEL-[a-z0-9]+(-[a-z0-9]+)*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal const string NovelMarker = "NOVEL-";

        /// <summary>
        /// Returns true when <paramref name="ruleId"/> starts with the NOVEL- escape-hatch
        /// prefix. The full grammar is enforced by <see cref="IsAcceptable"/>; this helper
        /// is for consumers (e.g., the AI1012 validation rule) that just need to know
        /// whether the ruleId is a NOVEL- finding and therefore already sub-id-bearing by
        /// convention.
        /// </summary>
        public static bool IsNovel(string ruleId)
        {
            return !string.IsNullOrEmpty(ruleId)
                && ruleId.StartsWith(NovelMarker, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns true when <paramref name="ruleId"/> conforms to one of the two AI ruleId
        /// shapes (taxonomy sub-id or NOVEL- prefix). Null and empty are rejected.
        /// </summary>
        public static bool IsAcceptable(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                return false;
            }

            if (IsNovel(ruleId))
            {
                // NOVEL- prefix is exclusive: must match the flat NOVEL- form (no slash).
                return s_novelPrefix.IsMatch(ruleId);
            }

            return s_taxonomySubId.IsMatch(ruleId);
        }

        /// <summary>
        /// Throws <see cref="AIRuleIdConventionException"/> if <paramref name="ruleId"/>
        /// does not conform. The thrown message is shaped for AI consumption: it states
        /// what was rejected, why, and exactly which two forms are accepted.
        /// </summary>
        public static void ThrowIfUnacceptable(string ruleId)
        {
            if (!IsAcceptable(ruleId))
            {
                throw new AIRuleIdConventionException(new[] { ruleId });
            }
        }

        /// <summary>
        /// Validates every result's <see cref="Result.RuleId"/>. If any violate the convention,
        /// throws a single <see cref="AIRuleIdConventionException"/> that lists ALL offenders
        /// so an AI orchestrator can correct them in one round trip rather than discovering
        /// them one at a time.
        /// </summary>
        public static void ThrowIfAnyUnacceptable(IList<Result> results)
        {
            if (results == null || results.Count == 0)
            {
                return;
            }

            List<string> offenders = null;
            for (int i = 0; i < results.Count; i++)
            {
                string ruleId = results[i]?.RuleId;
                if (!IsAcceptable(ruleId))
                {
                    offenders ??= new List<string>();
                    offenders.Add(ruleId ?? string.Empty);
                }
            }

            if (offenders != null)
            {
                throw new AIRuleIdConventionException(offenders);
            }
        }
    }
}
