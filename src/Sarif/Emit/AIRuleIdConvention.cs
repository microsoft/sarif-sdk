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
    /// <item><description><b>Taxonomy sub-id</b> — <c>&lt;BASE&gt;/&lt;sub-id&gt;</c> where
    /// <c>BASE</c> is a recognized taxonomy entry id (e.g., <c>CWE-89</c>,
    /// <c>CVE-2021-12345</c>, <c>OWASP-A01-2021</c>) and <c>sub-id</c> is a non-empty
    /// AI-chosen sub-classifier with no slashes or whitespace
    /// (e.g., <c>CWE-89/kql-injection-from-config</c>).</description></item>
    /// <item><description><b>NOVEL escape hatch</b> — <c>NOVEL-&lt;sub-id&gt;</c> for
    /// findings that don't map to any known taxonomy entry
    /// (e.g., <c>NOVEL-prompt-injection-via-system-message</c>). The NOVEL- form is
    /// exclusive: it does not accept a slash. If the AI can connect the finding back to
    /// a taxonomy entry it MUST use the sub-id form instead.</description></item>
    /// </list>
    /// <para>Rationale: the sub-id form keeps AI1012 silent (sub-classification is what
    /// the rule wants) AND lets the CWE taxonomy enricher hydrate the base descriptor
    /// from MITRE metadata, so the AI gets enriched output for free while staying
    /// honest about which sub-pattern of the base it observed. The NOVEL- form keeps
    /// non-taxonomy findings emittable without forcing the AI to pretend a CWE applies.
    /// See <c>docs/AI-RuleId-Convention.md</c> for the full rationale and examples.</para>
    /// <para>Producers using <see cref="Writers.SarifLogger"/> directly do not flow through
    /// this convention — it is specific to the AI-authoring emit verb path.</para>
    /// </remarks>
    public static class AIRuleIdConvention
    {
        // Taxonomy sub-id form: uppercase taxonomy prefix (CWE, CVE, OWASP, ...) with one
        // or more dash-separated alphanumeric tokens, then a slash, then a non-empty sub-id
        // that contains no further slashes and no whitespace.
        private static readonly Regex s_taxonomySubId = new Regex(
            @"^[A-Z][A-Z0-9]*(-[A-Za-z0-9]+)+/[^/\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // NOVEL escape hatch: "NOVEL-" followed by one or more dash-separated alphanumeric
        // tokens. No slashes, no whitespace, no trailing dash.
        private static readonly Regex s_novelPrefix = new Regex(
            @"^NOVEL-[A-Za-z0-9]+(-[A-Za-z0-9]+)*$",
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
