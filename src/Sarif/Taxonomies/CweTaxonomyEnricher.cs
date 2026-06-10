// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Taxonomies
{
    /// <summary>
    /// Enriches <see cref="ReportingDescriptor"/> instances on a <see cref="Run"/> whose
    /// <c>id</c> matches a MITRE CWE entry, populating <c>name</c>, <c>shortDescription</c>,
    /// <c>fullDescription</c>, <c>helpUri</c>, and <c>help</c> from the SDK's embedded
    /// taxonomy artifacts.
    /// </summary>
    /// <remarks>
    /// <para>Producer-supplied descriptor fields are never overwritten.</para>
    /// <para>The taxonomy omits <c>shortDescription</c> for taxa whose value is the
    /// first sentence of <c>fullDescription</c> (SARIF §3.49.10). When such a taxon
    /// enriches a descriptor, this enricher derives <c>shortDescription</c> from the
    /// first sentence of the copied <c>fullDescription</c> so consumers that require an
    /// explicit short description still receive one.</para>
    /// <para>This enricher does not add cross-references via
    /// <c>reportingDescriptor.relationships</c> or <c>result.taxa</c>.</para>
    /// </remarks>
    public static class CweTaxonomyEnricher
    {
        private const string CweHelpUriFormat = "https://cwe.mitre.org/data/definitions/{0}.html";

        // Match descriptor ids only; result-level sub-id forms such as "CWE-79/api-handler"
        // are not descriptor ids per SARIF §3.52.4.
        private static readonly Regex CweIdPattern =
            new Regex(@"^\s*[Cc][Ww][Ee]-(\d+)\s*$", RegexOptions.CultureInvariant);

        // First-sentence terminator. Mirrors the dead-simple rule the taxonomy
        // generator uses to decide whether shortDescription is recoverable from
        // fullDescription: the first sentence-ending punctuation, any trailing
        // closing quote/paren/bracket, then whitespace or end of string.
        private static readonly Regex SentenceEndPattern =
            new Regex(@"[.!?][""')\]]*(\s|$)", RegexOptions.CultureInvariant);

        /// <summary>
        /// Enriches every descriptor on the supplied <see cref="Run"/> whose id maps to a
        /// CWE entry in the requested statuses.
        /// </summary>
        /// <param name="run">The run whose <c>tool.driver.rules</c> and <c>tool.extensions[].rules</c> are enriched.</param>
        /// <param name="statuses">
        /// The CWE statuses to source enrichment data from. Defaults to <see cref="CweTaxonomy.DefaultStatuses"/>
        /// (<c>Stable | Draft | Incomplete</c>), which excludes <see cref="CweStatus.Deprecated"/>.
        /// </param>
        /// <returns>The number of descriptors whose content was modified.</returns>
        public static int Enrich(Run run, CweStatus statuses = CweTaxonomy.DefaultStatuses)
        {
            if (run == null) { throw new ArgumentNullException(nameof(run)); }

            IDictionary<string, ReportingDescriptor> taxa = BuildTaxaLookup(statuses);
            int modified = 0;

            if (run.Tool?.Driver?.Rules != null)
            {
                foreach (ReportingDescriptor rule in run.Tool.Driver.Rules)
                {
                    if (TryEnrich(rule, taxa)) { modified++; }
                }
            }

            if (run.Tool?.Extensions != null)
            {
                foreach (ToolComponent extension in run.Tool.Extensions)
                {
                    if (extension?.Rules == null) { continue; }
                    foreach (ReportingDescriptor rule in extension.Rules)
                    {
                        if (TryEnrich(rule, taxa)) { modified++; }
                    }
                }
            }

            return modified;
        }

        private static bool TryNormalizeCweId(string id, out string canonical)
        {
            canonical = null;
            if (string.IsNullOrWhiteSpace(id)) { return false; }

            Match match = CweIdPattern.Match(id);
            if (!match.Success) { return false; }

            canonical = "CWE-" + match.Groups[1].Value;
            return true;
        }

        private static bool TryEnrich(ReportingDescriptor rule, IDictionary<string, ReportingDescriptor> taxa)
        {
            if (rule?.Id == null) { return false; }
            if (!TryNormalizeCweId(rule.Id, out string canonical)) { return false; }
            if (!taxa.TryGetValue(canonical, out ReportingDescriptor taxon)) { return false; }

            bool modified = false;

            if (string.IsNullOrEmpty(rule.Name) && !string.IsNullOrEmpty(taxon.Name))
            {
                rule.Name = taxon.Name;
                modified = true;
            }

            if (IsEmptyMessage(rule.ShortDescription) && !IsEmptyMessage(taxon.ShortDescription))
            {
                rule.ShortDescription = CloneMessage(taxon.ShortDescription);
                modified = true;
            }

            if (IsEmptyMessage(rule.FullDescription) && !IsEmptyMessage(taxon.FullDescription))
            {
                rule.FullDescription = CloneMessage(taxon.FullDescription);
                modified = true;
            }

            if (IsEmptyMessage(rule.ShortDescription) && !string.IsNullOrEmpty(rule.FullDescription?.Text))
            {
                rule.ShortDescription = new MultiformatMessageString
                {
                    Text = DeriveShortDescription(rule.FullDescription.Text),
                };
                modified = true;
            }

            if (string.IsNullOrEmpty(rule.HelpUri?.OriginalString))
            {
                string helpUri = !string.IsNullOrEmpty(taxon.HelpUri?.OriginalString)
                    ? taxon.HelpUri.OriginalString
                    : string.Format(System.Globalization.CultureInfo.InvariantCulture, CweHelpUriFormat, canonical.Substring(4));
                rule.HelpUri = new Uri(helpUri, UriKind.Absolute);
                modified = true;
            }

            if (IsEmptyMultiformat(rule.Help) && !IsEmptyMultiformat(taxon.Help))
            {
                rule.Help = CloneMultiformat(taxon.Help);
                modified = true;
            }

            return modified;
        }

        private static IDictionary<string, ReportingDescriptor> BuildTaxaLookup(CweStatus statuses)
        {
            var taxa = new Dictionary<string, ReportingDescriptor>(StringComparer.Ordinal);
            SarifLog log = CweTaxonomy.Load(statuses);
            if (log?.Runs == null) { return taxa; }

            foreach (Run run in log.Runs)
            {
                if (run?.Taxonomies == null) { continue; }
                foreach (ToolComponent taxonomy in run.Taxonomies)
                {
                    if (taxonomy?.Taxa == null) { continue; }
                    foreach (ReportingDescriptor entry in taxonomy.Taxa)
                    {
                        if (!string.IsNullOrEmpty(entry?.Id))
                        {
                            taxa[entry.Id] = entry;
                        }
                    }
                }
            }
            return taxa;
        }

        private static bool IsEmptyMessage(MultiformatMessageString message)
        {
            return message == null || (string.IsNullOrEmpty(message.Text) && string.IsNullOrEmpty(message.Markdown));
        }

        private static string DeriveShortDescription(string fullDescriptionText)
        {
            Match match = SentenceEndPattern.Match(fullDescriptionText);
            string candidate = match.Success
                ? fullDescriptionText.Substring(0, match.Index + match.Length)
                : fullDescriptionText;
            return candidate.Trim();
        }

        private static MultiformatMessageString CloneMessage(MultiformatMessageString source)
        {
            return new MultiformatMessageString
            {
                Text = source.Text,
                Markdown = source.Markdown,
            };
        }

        private static bool IsEmptyMultiformat(MultiformatMessageString message)
        {
            return IsEmptyMessage(message);
        }

        private static MultiformatMessageString CloneMultiformat(MultiformatMessageString source)
        {
            return CloneMessage(source);
        }
    }
}
