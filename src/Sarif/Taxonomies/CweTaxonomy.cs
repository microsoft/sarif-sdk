// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Taxonomies
{
    /// <summary>
    /// Provides access to the SDK's embedded MITRE CWE taxonomy in SARIF and
    /// compact-markdown form. Callers select a subset by <see cref="CweStatus"/>;
    /// the default (<see cref="DefaultStatuses"/>) is <c>Stable | Draft | Incomplete</c>,
    /// which mirrors what real-world scanners report — see remarks for the rationale.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The SDK ships exactly two embedded resources — one consolidated SARIF taxonomy
    /// (<c>CweTaxonomy.sarif</c>) and one consolidated markdown table (<c>CweTaxonomy.brief.md</c>) —
    /// containing every entry in the upstream MITRE catalog regardless of status.
    /// Each taxon carries its <c>cwe/status</c> as a property, and the brief table has
    /// a Status column. Filtering by status happens at read time, never at load time.
    /// </para>
    /// <para>
    /// Sized for AI prompt-context injection: the brief table fits ~60K tokens at the
    /// default loadout, comfortable for every modern frontier model.
    /// </para>
    /// </remarks>
    public static class CweTaxonomy
    {
        /// <summary>
        /// The default set of CWE statuses for read and enrichment operations:
        /// <see cref="CweStatus.Stable"/> | <see cref="CweStatus.Draft"/> | <see cref="CweStatus.Incomplete"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Notably <em>includes</em> <see cref="CweStatus.Incomplete"/> and <em>excludes</em>
        /// <see cref="CweStatus.Deprecated"/>. This is the non-obvious shape, and it is deliberate.
        /// </para>
        /// <para>
        /// MITRE's "Stable" bar is much higher than common usage suggests — at cwec_v4.20
        /// only 26 of 969 entries are Stable. Most household-name CWEs (XXE, deserialization,
        /// hardcoded credentials, broken crypto, out-of-bounds write) are still <em>Draft</em>,
        /// and SSRF (CWE-918) — an OWASP Top 10 entry since 2021 — is <em>Incomplete</em>.
        /// </para>
        /// <para>
        /// We measured how often Incomplete CWEs show up in real scanner rule metadata
        /// across <c>github/codeql</c> (13,143 query files) and <c>semgrep/semgrep-rules</c>
        /// (2,183 rule files): of 349 distinct CWEs cited, <strong>136 (39%) are upstream-Incomplete</strong>
        /// — including CWE-1220 (Insufficient Granularity of Access Control), the third-most-cited
        /// CWE across all of Semgrep at 108 rule files. Defaulting to <c>Stable | Draft</c> would
        /// silently exclude two-fifths of what real scanners actually emit. See
        /// <c>src/Sarif/Taxonomies/CweReadme.md</c> for the full table and methodology.
        /// </para>
        /// <para>
        /// Excluding <c>Deprecated</c> by default is also intentional and also measured: across
        /// those same 349 cited CWEs, exactly one Deprecated CWE appears (CWE-247, once). Real
        /// scanners have already migrated off deprecated CWEs. The enricher
        /// (<see cref="CweTaxonomyEnricher"/>) intentionally gives no help on a deprecated CWE,
        /// leaving the descriptor's metadata empty so the producer notices and migrates to the
        /// MITRE-recommended replacement. Callers that want a complete snapshot can pass
        /// <see cref="CweStatus.All"/>.
        /// </para>
        /// </remarks>
        public const CweStatus DefaultStatuses = CweStatus.Stable | CweStatus.Draft | CweStatus.Incomplete;

        private const string SarifResourceName = "Microsoft.CodeAnalysis.Sarif.Taxonomies.CweTaxonomy.sarif";
        private const string BriefResourceName = "Microsoft.CodeAnalysis.Sarif.Taxonomies.CweTaxonomy.brief.md";

        private const string StatusPropertyName = "cwe/status";
        private const string AbstractionPropertyName = "cwe/abstraction";
        private const string ParentPropertyName = "cwe/parent";

        private static readonly object Gate = new object();
        private static SarifLog canonicalLog;
        private static string canonicalBrief;
        private static HashSet<int> weaknessNumbers;

        // First-sentence terminator: the first sentence-ending punctuation, any
        // trailing closing quote/paren/bracket, then whitespace or end of string.
        // This is the dead-simple rule the taxonomy generator uses to decide
        // whether shortDescription is recoverable from fullDescription, and that
        // consumers use to recover it (SARIF §3.49.10). It is deliberately naive:
        // the generator only omits shortDescription when this rule reproduces the
        // curated first sentence exactly, so consumers never need a smarter parser.
        private static readonly Regex SentenceEndPattern =
            new Regex(@"[.!?][""')\]]*(\s|$)", RegexOptions.CultureInvariant);

        /// <summary>
        /// Loads the consolidated CWE taxonomy, optionally filtered by status.
        /// </summary>
        /// <param name="statuses">
        /// Bitwise combination of <see cref="CweStatus"/> flags. Defaults to <see cref="DefaultStatuses"/>
        /// (<c>Stable | Draft | Incomplete</c>) — see the documentation on <see cref="DefaultStatuses"/>
        /// for why this loadout is the right baseline.
        /// </param>
        /// <returns>
        /// A <see cref="SarifLog"/> whose <c>runs[0].taxonomies[0].taxa</c> contains every CWE
        /// entry whose status matches one of the requested flags. Returns the canonical log
        /// directly (no filtering, no copy) when <paramref name="statuses"/> is <see cref="CweStatus.All"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="statuses"/> is <see cref="CweStatus.None"/>.</exception>
        public static SarifLog Load(CweStatus statuses = DefaultStatuses)
        {
            if (statuses == CweStatus.None)
            {
                throw new ArgumentException("At least one CweStatus flag must be specified.", nameof(statuses));
            }

            SarifLog canonical = LoadCanonical();
            if (statuses == CweStatus.All) { return canonical; }

            HashSet<string> wantedStatusNames = StatusNamesFromFlags(statuses);
            return BuildFilteredLog(canonical, wantedStatusNames);
        }

        /// <summary>
        /// Loads the compact markdown table of CWE entries, optionally filtered by status.
        /// </summary>
        /// <param name="statuses">
        /// Bitwise combination of <see cref="CweStatus"/> flags. Defaults to <see cref="DefaultStatuses"/>.
        /// </param>
        /// <returns>
        /// The verbatim embedded canonical string when <paramref name="statuses"/> is <see cref="CweStatus.All"/>;
        /// otherwise a re-rendered table with only the matching rows.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="statuses"/> is <see cref="CweStatus.None"/>.</exception>
        public static string LoadBrief(CweStatus statuses = DefaultStatuses)
        {
            if (statuses == CweStatus.None)
            {
                throw new ArgumentException("At least one CweStatus flag must be specified.", nameof(statuses));
            }

            if (statuses == CweStatus.All) { return LoadCanonicalBrief(); }

            HashSet<string> wantedStatusNames = StatusNamesFromFlags(statuses);
            return RenderBrief(LoadCanonical(), wantedStatusNames);
        }

        /// <summary>
        /// Recovers the first sentence of a CWE <c>fullDescription</c>, the value a consumer
        /// displays as <c>shortDescription</c> when the taxonomy omits one (SARIF §3.49.10).
        /// </summary>
        /// <param name="fullDescriptionText">The <c>fullDescription</c> text to recover from.</param>
        /// <returns>
        /// The leading sentence (through its terminating punctuation), trimmed; the whole
        /// trimmed input when no sentence terminator is present; <c>null</c> for null input.
        /// </returns>
        public static string DeriveShortDescription(string fullDescriptionText)
        {
            if (fullDescriptionText == null) { return null; }

            Match match = SentenceEndPattern.Match(fullDescriptionText);
            string candidate = match.Success
                ? fullDescriptionText.Substring(0, match.Index + match.Length)
                : fullDescriptionText;
            return candidate.Trim();
        }

        /// <summary>
        /// Determines whether a CWE identifier names a known MITRE <em>Weakness</em> — the only
        /// abstraction that is a valid <c>result.ruleId</c> mapping target. Returns <c>false</c>
        /// for a Category, a View, a withdrawn id, a typo, or any non-CWE token. Membership is
        /// evaluated across every maturity status (a deprecated Weakness is still a Weakness).
        /// </summary>
        /// <param name="cweId">
        /// A CWE identifier in any form the SDK accepts: a canonical id (<c>CWE-89</c>, any case,
        /// leading zeros tolerated) or an AI ruleId carrying a sub-id (<c>CWE-89/kql-injection</c>).
        /// The <c>CWE-</c> prefix is required; a bare number or the <c>NOVEL-</c> form yields <c>false</c>.
        /// </param>
        /// <returns><c>true</c> when the id resolves to an embedded CWE Weakness; otherwise <c>false</c>.</returns>
        public static bool IsKnownWeakness(string cweId)
        {
            return CweSecuritySeverity.TryGetCweNumber(cweId, out int cweNumber)
                && WeaknessNumbers().Contains(cweNumber);
        }

        internal static SarifLog LoadCanonical()
        {
            lock (Gate)
            {
                if (canonicalLog == null)
                {
                    using (Stream stream = OpenResource(SarifResourceName))
                    {
                        canonicalLog = SarifLog.Load(stream);
                    }
                }
                return canonicalLog;
            }
        }

        private static HashSet<int> WeaknessNumbers()
        {
            lock (Gate)
            {
                if (weaknessNumbers == null)
                {
                    var set = new HashSet<int>();
                    foreach (ReportingDescriptor taxon in LoadCanonical().Runs[0].Taxonomies[0].Taxa)
                    {
                        if (CweSecuritySeverity.TryGetCweNumber(taxon.Id, out int cweNumber))
                        {
                            set.Add(cweNumber);
                        }
                    }
                    weaknessNumbers = set;
                }
                return weaknessNumbers;
            }
        }

        internal static string LoadCanonicalBrief()
        {
            lock (Gate)
            {
                if (canonicalBrief == null)
                {
                    using (Stream stream = OpenResource(BriefResourceName))
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        canonicalBrief = reader.ReadToEnd();
                    }
                }
                return canonicalBrief;
            }
        }

        internal static HashSet<string> StatusNamesFromFlags(CweStatus statuses)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if ((statuses & CweStatus.Stable) != 0) { set.Add("Stable"); }
            if ((statuses & CweStatus.Draft) != 0) { set.Add("Draft"); }
            if ((statuses & CweStatus.Incomplete) != 0) { set.Add("Incomplete"); }
            if ((statuses & CweStatus.Deprecated) != 0) { set.Add("Deprecated"); }
            if ((statuses & CweStatus.Obsolete) != 0) { set.Add("Obsolete"); }
            return set;
        }

        private static SarifLog BuildFilteredLog(SarifLog canonical, HashSet<string> wantedStatusNames)
        {
            Run canonicalRun = canonical.Runs[0];
            ToolComponent canonicalTaxonomy = canonicalRun.Taxonomies[0];

            var filteredTaxa = new List<ReportingDescriptor>();
            foreach (ReportingDescriptor taxon in canonicalTaxonomy.Taxa)
            {
                if (TaxonMatchesStatus(taxon, wantedStatusNames))
                {
                    filteredTaxa.Add(taxon);
                }
            }

            var filteredTaxonomy = new ToolComponent
            {
                Name = canonicalTaxonomy.Name,
                Version = canonicalTaxonomy.Version,
                Organization = canonicalTaxonomy.Organization,
                InformationUri = canonicalTaxonomy.InformationUri,
                DownloadUri = canonicalTaxonomy.DownloadUri,
                IsComprehensive = false,
                MinimumRequiredLocalizedDataSemanticVersion = canonicalTaxonomy.MinimumRequiredLocalizedDataSemanticVersion,
                ShortDescription = canonicalTaxonomy.ShortDescription,
                FullDescription = canonicalTaxonomy.FullDescription,
                Taxa = filteredTaxa,
            };

            return new SarifLog
            {
                SchemaUri = canonical.SchemaUri,
                Version = canonical.Version,
                Runs = new List<Run>
                {
                    new Run
                    {
                        Tool = canonicalRun.Tool,
                        Taxonomies = new List<ToolComponent> { filteredTaxonomy },
                    },
                },
            };
        }

        private static bool TaxonMatchesStatus(ReportingDescriptor taxon, HashSet<string> wantedStatusNames)
        {
            if (taxon == null) { return false; }
            return taxon.TryGetProperty(StatusPropertyName, out string status) && wantedStatusNames.Contains(status);
        }

        private static string RenderBrief(SarifLog canonical, HashSet<string> wantedStatusNames)
        {
            ReportingDescriptor[] taxa = canonical.Runs[0].Taxonomies[0].Taxa
                .Where(t => TaxonMatchesStatus(t, wantedStatusNames))
                .ToArray();

            var sb = new StringBuilder();
            sb.AppendLine("# CWE").AppendLine();
            sb.AppendLine(string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "Compact one-row-per-entry index of the MITRE CWE catalog. {0} entries (filtered).",
                taxa.Length)).AppendLine();
            sb.AppendLine("| ID | Name | Abstraction | Status | Parent | Description |");
            sb.AppendLine("|----|------|-------------|--------|--------|-------------|");

            foreach (ReportingDescriptor t in taxa)
            {
                string status = GetPropertyString(t, StatusPropertyName);
                string abstraction = GetPropertyString(t, AbstractionPropertyName);
                string parent = GetPropertyString(t, ParentPropertyName);
                string shortText = t.ShortDescription?.Text;
                if (string.IsNullOrEmpty(shortText) && !string.IsNullOrEmpty(t.FullDescription?.Text))
                {
                    shortText = DeriveShortDescription(t.FullDescription.Text);
                }
                string desc = (shortText ?? string.Empty).Replace("|", @"\|").Replace("\r", " ").Replace("\n", " ");
                string name = (t.Name ?? string.Empty).Replace("|", @"\|").Replace("\r", " ").Replace("\n", " ").Trim();
                sb.AppendLine($"| {t.Id} | {name} | {abstraction} | {status} | {parent} | {desc} |");
            }

            return sb.ToString();
        }

        private static string GetPropertyString(ReportingDescriptor t, string propertyName)
        {
            return t != null && t.TryGetProperty(propertyName, out string value) ? value : string.Empty;
        }

        private static Stream OpenResource(string resourceName)
        {
            Assembly assembly = typeof(CweTaxonomy).Assembly;
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new InvalidOperationException(
                    "Embedded CWE taxonomy resource '" + resourceName + "' was not found in assembly '"
                    + assembly.FullName + "'.");
            }
            return stream;
        }
    }
}
