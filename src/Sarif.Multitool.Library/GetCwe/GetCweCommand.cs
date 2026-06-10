// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Microsoft.CodeAnalysis.Sarif.Taxonomies;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>get-cwe</c>: serves canonical MITRE CWE data from the SDK's embedded taxonomy.
    /// </summary>
    /// <remarks>
    /// Each record's <c>ruleIdFallback</c> (<c>CWE-&lt;n&gt;/&lt;slug&gt;</c>) is the kebab-cased
    /// CWE name produced by the same helper AI1012 uses, so the two always agree.
    /// </remarks>
    public class GetCweCommand : CommandBase
    {
        internal const string CweTitleProperty = "cwe/title";
        internal const string CweStatusProperty = "cwe/status";
        internal const string CweAbstractionProperty = "cwe/abstraction";
        internal const string CweParentProperty = "cwe/parent";

        private const string HelpUriFormat = "https://cwe.mitre.org/data/definitions/{0}.html";

        public int Run(GetCweOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                bool hasIds = !string.IsNullOrWhiteSpace(options.Ids);

                if (hasIds && options.All)
                {
                    Console.Error.WriteLine("error: specify CWE ids or --all, not both.");
                    return FAILURE;
                }

                if (!hasIds && !options.All)
                {
                    Console.Error.WriteLine("error: specify one or more CWE ids (e.g. '89,CWE-79'), or pass --all.");
                    return FAILURE;
                }

                IReadOnlyList<ReportingDescriptor> selected;
                if (options.All)
                {
                    selected = SelectAll();
                }
                else
                {
                    selected = SelectByIds(options.Ids, LoadTaxaById(), out string selectionError);
                    if (selected == null)
                    {
                        Console.Error.WriteLine(selectionError);
                        return FAILURE;
                    }
                }

                var records = new List<CweRecord>(selected.Count);
                var invariantOffenders = new List<string>();
                foreach (ReportingDescriptor taxon in selected)
                {
                    CweRecord record = BuildRecord(taxon, options.Concise, invariantOffenders);
                    if (record != null) { records.Add(record); }
                }

                if (invariantOffenders.Count > 0)
                {
                    Console.Error.WriteLine(
                        "error: the embedded CWE taxonomy yielded an unusable slug for: "
                        + string.Join(", ", invariantOffenders) + ".");
                    return FAILURE;
                }

                string content = options.Format == CweOutputFormat.Markdown
                    ? RenderMarkdown(records)
                    : RenderJson(records);

                if (!string.IsNullOrEmpty(options.OutputFilePath))
                {
                    if (fileSystem.FileExists(options.OutputFilePath) && !options.ForceOverwrite)
                    {
                        Console.Error.WriteLine(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "error: '{0}' already exists. Pass --force-overwrite to replace it.",
                                options.OutputFilePath));
                        return FAILURE;
                    }

                    fileSystem.FileWriteAllText(options.OutputFilePath, content);
                    Console.Out.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Wrote {0} CWE record(s) to '{1}'.",
                            records.Count,
                            options.OutputFilePath));
                    return SUCCESS;
                }

                Console.Out.WriteLine(content);
                return SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }

        // Every embedded entry is loaded regardless of status: a caller who names a deprecated CWE
        // still gets it, and --all means the whole catalog.
        private static IReadOnlyDictionary<string, ReportingDescriptor> LoadTaxaById()
        {
            SarifLog log = CweTaxonomy.Load(CweStatus.All);
            var byId = new Dictionary<string, ReportingDescriptor>(StringComparer.Ordinal);
            foreach (ReportingDescriptor taxon in log.Runs[0].Taxonomies[0].Taxa)
            {
                if (!string.IsNullOrEmpty(taxon?.Id)) { byId[taxon.Id] = taxon; }
            }
            return byId;
        }

        private static IReadOnlyList<ReportingDescriptor> SelectAll()
        {
            // Canonical taxonomy order.
            return CweTaxonomy.Load(CweStatus.All).Runs[0].Taxonomies[0].Taxa
                .Where(t => !string.IsNullOrEmpty(t?.Id))
                .ToList();
        }

        // Returns null and sets 'error' on the first decisive failure (malformed token, or a
        // well-formed id that resolves to no entry). On success, returns the resolved taxa in the
        // caller's first-requested order with duplicates collapsed.
        private static IReadOnlyList<ReportingDescriptor> SelectByIds(
            string rawIds,
            IReadOnlyDictionary<string, ReportingDescriptor> taxaById,
            out string error)
        {
            error = null;

            string[] tokens = rawIds.Split(',');
            var canonicalOrder = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var malformed = new List<string>();

            foreach (string token in tokens)
            {
                string trimmed = token.Trim();
                if (!TryNormalizeCweId(trimmed, out string canonical))
                {
                    malformed.Add(trimmed.Length == 0 ? "(empty)" : trimmed);
                    continue;
                }

                if (seen.Add(canonical)) { canonicalOrder.Add(canonical); }
            }

            if (malformed.Count > 0)
            {
                error = "error: not a CWE id: " + string.Join(", ", malformed)
                    + ". Use a number or 'CWE-<number>' (e.g. '89' or 'CWE-89').";
                return null;
            }

            var resolved = new List<ReportingDescriptor>(canonicalOrder.Count);
            var notFound = new List<string>();
            foreach (string canonical in canonicalOrder)
            {
                if (taxaById.TryGetValue(canonical, out ReportingDescriptor taxon))
                {
                    resolved.Add(taxon);
                }
                else
                {
                    notFound.Add(canonical);
                }
            }

            if (notFound.Count > 0)
            {
                error = "error: no such CWE: " + string.Join(", ", notFound) + ".";
                return null;
            }

            return resolved;
        }

        // Accepts '89', 'CWE-89', 'cwe-89' (any case), with surrounding whitespace already trimmed.
        // Leading zeros are normalized away ('CWE-089' -> 'CWE-89'). Anything else is malformed.
        internal static bool TryNormalizeCweId(string token, out string canonical)
        {
            canonical = null;
            if (string.IsNullOrEmpty(token)) { return false; }

            string digits = token;
            if (digits.StartsWith("CWE-", StringComparison.OrdinalIgnoreCase))
            {
                digits = digits.Substring(4);
            }

            if (digits.Length == 0 || !digits.All(char.IsDigit)) { return false; }
            if (!int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int number)) { return false; }

            canonical = "CWE-" + number.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        private static CweRecord BuildRecord(ReportingDescriptor taxon, bool concise, List<string> invariantOffenders)
        {
            string slug = ProvideRuleSubId.ToKebabCase(taxon.Name);
            if (string.IsNullOrEmpty(slug))
            {
                invariantOffenders.Add(taxon.Id);
                return null;
            }

            return new CweRecord
            {
                Id = taxon.Id,
                Name = taxon.Name,
                Title = GetProperty(taxon, CweTitleProperty),
                Slug = slug,
                RuleIdFallback = taxon.Id + "/" + slug,
                Status = GetProperty(taxon, CweStatusProperty),
                Abstraction = GetProperty(taxon, CweAbstractionProperty),
                Parent = GetProperty(taxon, CweParentProperty),
                ShortDescription = ResolveShortDescription(taxon),
                HelpUri = ResolveHelpUri(taxon),
                Help = concise ? null : (taxon.Help?.Markdown ?? taxon.Help?.Text),
            };
        }

        // The taxonomy omits shortDescription whenever it is recoverable as the first sentence
        // of fullDescription (SARIF §3.49.10). get-cwe is such a consumer: it recovers the short
        // so every served record carries one.
        private static string ResolveShortDescription(ReportingDescriptor taxon)
        {
            if (!string.IsNullOrEmpty(taxon.ShortDescription?.Text)) { return taxon.ShortDescription.Text; }

            string full = taxon.FullDescription?.Text;
            return string.IsNullOrEmpty(full) ? null : CweTaxonomy.DeriveShortDescription(full);
        }

        private static string ResolveHelpUri(ReportingDescriptor taxon)
        {
            string declared = taxon.HelpUri?.OriginalString;
            if (!string.IsNullOrEmpty(declared)) { return declared; }

            string number = taxon.Id.StartsWith("CWE-", StringComparison.Ordinal)
                ? taxon.Id.Substring(4)
                : taxon.Id;
            return string.Format(CultureInfo.InvariantCulture, HelpUriFormat, number);
        }

        private static string GetProperty(ReportingDescriptor taxon, string name)
        {
            return taxon.TryGetProperty(name, out string value) ? value : null;
        }

        private static string RenderJson(IReadOnlyList<CweRecord> records)
        {
            return JsonConvert.SerializeObject(records, Formatting.Indented);
        }

        private static string RenderMarkdown(IReadOnlyList<CweRecord> records)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < records.Count; i++)
            {
                CweRecord r = records[i];
                if (i > 0) { sb.AppendLine(); }

                sb.AppendLine($"## {r.Id} — {r.Name}").AppendLine();
                if (!string.IsNullOrEmpty(r.Title)) { sb.AppendLine($"> {r.Title}").AppendLine(); }

                sb.AppendLine($"- **slug**: `{r.Slug}`");
                sb.AppendLine($"- **ruleId fallback**: `{r.RuleIdFallback}`");
                if (!string.IsNullOrEmpty(r.Status)) { sb.AppendLine($"- **status**: {r.Status}"); }
                if (!string.IsNullOrEmpty(r.Abstraction)) { sb.AppendLine($"- **abstraction**: {r.Abstraction}"); }
                if (!string.IsNullOrEmpty(r.Parent)) { sb.AppendLine($"- **parent**: {r.Parent}"); }
                if (!string.IsNullOrEmpty(r.HelpUri)) { sb.AppendLine($"- **help**: {r.HelpUri}"); }

                if (!string.IsNullOrEmpty(r.ShortDescription))
                {
                    sb.AppendLine().AppendLine(r.ShortDescription);
                }

                if (!string.IsNullOrEmpty(r.Help))
                {
                    sb.AppendLine().AppendLine(r.Help);
                }
            }

            return sb.ToString().TrimEnd() + Environment.NewLine;
        }

        // The serialized shape get-cwe emits. Property order here is the JSON field order.
        private sealed class CweRecord
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            [JsonProperty("slug")]
            public string Slug { get; set; }

            [JsonProperty("ruleIdFallback")]
            public string RuleIdFallback { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public string Status { get; set; }

            [JsonProperty("abstraction", NullValueHandling = NullValueHandling.Ignore)]
            public string Abstraction { get; set; }

            [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
            public string Parent { get; set; }

            [JsonProperty("shortDescription", NullValueHandling = NullValueHandling.Ignore)]
            public string ShortDescription { get; set; }

            [JsonProperty("helpUri", NullValueHandling = NullValueHandling.Ignore)]
            public string HelpUri { get; set; }

            [JsonProperty("help", NullValueHandling = NullValueHandling.Ignore)]
            public string Help { get; set; }
        }
    }
}
