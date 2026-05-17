// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// Resolves CWE identifiers (e.g., <c>"CWE-78"</c>) against the
    /// embedded CWE taxonomy (<c>src/Sarif/Taxonomies/CWE.sarif</c>,
    /// MITRE View-1000). Returns a PascalCase name suitable for use as
    /// <c>reportingDescriptor.name</c> on an AI rule.
    /// </summary>
    /// <remarks>
    /// <para>
    /// AI security tools cite CWEs as first-order rules (the
    /// <c>reportingDescriptor.id</c> is the CWE id), so the MCP server needs a
    /// name + description lookup to populate the rule descriptor at registration
    /// time. Earlier revisions fetched this data from <c>cwe.mitre.org</c> per
    /// CWE id at runtime; that's needless network reliance for a slowly-changing
    /// enumeration. This implementation reads the taxonomy from an embedded
    /// resource at first use and serves all subsequent lookups in-memory.
    /// </para>
    /// <para>
    /// Regenerate the underlying taxonomy via
    /// <c>scripts/Generate-CweTaxonomy.ps1</c> when MITRE publishes a new CWE
    /// catalog version.
    /// </para>
    /// </remarks>
    public sealed class CweNameResolver
    {
        /// <summary>
        /// Manifest resource name of the embedded CWE taxonomy. Exposed so
        /// consumers (and tests) can reach the embedded SARIF file directly
        /// without re-discovering the resource path.
        /// </summary>
        public const string EmbeddedResourceName =
            "Microsoft.CodeAnalysis.Sarif.Mcp.Server.Taxonomies.CWE.sarif";

        private readonly Lazy<IReadOnlyDictionary<string, string>> _idToName;

        public CweNameResolver()
        {
            this._idToName = new Lazy<IReadOnlyDictionary<string, string>>(LoadFromEmbeddedResource);
        }

        /// <summary>
        /// Resolves a CWE id (with or without the <c>CWE-</c> prefix) to a
        /// PascalCase name. Returns null for non-CWE inputs or unknown ids.
        /// </summary>
        public string? Resolve(string baseRuleId)
        {
            if (string.IsNullOrEmpty(baseRuleId))
            {
                return null;
            }

            if (!baseRuleId.StartsWith("CWE-", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string numericId = baseRuleId.Substring(4);
            return this._idToName.Value.TryGetValue(numericId, out string? name) ? name : null;
        }

        private static IReadOnlyDictionary<string, string> LoadFromEmbeddedResource()
        {
            Assembly assembly = typeof(CweNameResolver).Assembly;
            using Stream? stream = assembly.GetManifestResourceStream(EmbeddedResourceName);

            if (stream == null)
            {
                throw new InvalidOperationException(
                    $"Embedded CWE taxonomy '{EmbeddedResourceName}' is missing from " +
                    $"'{assembly.GetName().Name}'. The csproj must include the taxonomy " +
                    "as an EmbeddedResource (see Sarif.Mcp.Server.csproj).");
            }

            SarifLog log = SarifLog.Load(stream);

            IList<ToolComponent>? taxonomies = log.Runs?[0]?.Taxonomies;
            if (taxonomies == null || taxonomies.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Embedded CWE taxonomy contains no taxonomies in run.taxonomies[].");
            }

            ToolComponent cwe = taxonomies[0];
            if (cwe.Taxa == null)
            {
                throw new InvalidOperationException(
                    $"Embedded CWE taxonomy '{cwe.Name}' contains no taxa.");
            }

            var lookup = new Dictionary<string, string>(cwe.Taxa.Count, StringComparer.OrdinalIgnoreCase);
            foreach (ReportingDescriptor taxon in cwe.Taxa)
            {
                if (string.IsNullOrEmpty(taxon.Id) || string.IsNullOrEmpty(taxon.Name))
                {
                    continue;
                }

                lookup[taxon.Id] = ToPascalCase(taxon.Name);
            }

            return lookup;
        }

        /// <summary>
        /// Converts the MITRE-canonical CWE name to a PascalCase identifier
        /// suitable for <c>reportingDescriptor.name</c> per SARIF \u00a73.49.7.
        /// </summary>
        /// <remarks>
        /// Many CWE entries carry an alternate name in a parenthetical
        /// suffix (e.g.,
        /// <c>"Improper Neutralization ... ('OS Command Injection')"</c>);
        /// the parenthetical part is a synonym, not additional information,
        /// so it's elided before PascalCasing to keep the identifier compact.
        /// </remarks>
        internal static string ToPascalCase(string name)
        {
            // Drop the parenthetical alternate-name suffix when present;
            // keep the canonical phrase before it.
            int paren = name.IndexOf('(');
            if (paren > 0)
            {
                name = name.Substring(0, paren).TrimEnd();
            }

            // Reject punctuation that PascalCase can't carry; keep
            // alphanumerics and split on everything else.
            var words = name.Split(
                new[] { ' ', '-', '_', '(', ')', '\'', '"', ':', '/', '.', ',', ';', '!', '?', '\u2013', '\u2014' },
                StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpperInvariant(words[i][0])
                        + (words[i].Length > 1 ? words[i].Substring(1).ToLowerInvariant() : string.Empty);
                }
            }

            return string.Concat(words);
        }
    }
}
