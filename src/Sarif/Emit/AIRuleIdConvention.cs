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
    /// <para>Accepted ruleId forms:</para>
    /// <list type="bullet">
    /// <item><description><c>CWE-&lt;number&gt;/&lt;sub-id&gt;</c>, where <c>sub-id</c> is lowercase
    /// alphanumeric kebab-case; for example, <c>CWE-89/kql-injection-from-config</c>.</description></item>
    /// <item><description><c>NOVEL-&lt;sub-id&gt;</c> for findings with no CWE mapping; the
    /// NOVEL- form is flat and does not accept a slash.</description></item>
    /// </list>
    /// <para>Producers using <see cref="Writers.SarifLogger"/> directly do not flow through
    /// this convention; it is specific to the AI-authoring emit verb path.</para>
    /// </remarks>
    public static class AIRuleIdConvention
    {
        // CWE sub-id form: CWE-<number>/<lowercase-alphanumeric-kebab-sub-id>.
        private static readonly Regex s_taxonomySubId = new Regex(
            @"^CWE-[0-9]+/[a-z0-9]+(-[a-z0-9]+)*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // NOVEL escape hatch: NOVEL-<lowercase-alphanumeric-kebab-sub-id>; no slash.
        private static readonly Regex s_novelPrefix = new Regex(
            @"^NOVEL-[a-z0-9]+(-[a-z0-9]+)*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal const string NovelMarker = "NOVEL-";

        /// <summary>
        /// Returns true when <paramref name="ruleId"/> starts with the NOVEL- escape-hatch
        /// prefix; the full grammar is enforced by <see cref="IsAcceptable"/>.
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
                return s_novelPrefix.IsMatch(ruleId);
            }

            return s_taxonomySubId.IsMatch(ruleId);
        }

        /// <summary>
        /// Throws <see cref="AIRuleIdConventionException"/> if <paramref name="ruleId"/>
        /// does not conform.
        /// </summary>
        public static void ThrowIfUnacceptable(string ruleId)
        {
            if (!IsAcceptable(ruleId))
            {
                throw new AIRuleIdConventionException(new[] { ruleId });
            }
        }

        /// <summary>
        /// Throws a single <see cref="AIRuleIdConventionException"/> listing every result whose
        /// <see cref="Result.RuleId"/> violates the convention.
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
