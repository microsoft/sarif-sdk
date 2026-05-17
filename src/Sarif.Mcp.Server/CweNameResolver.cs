// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// Fetches canonical CWE names from cwe.mitre.org and converts them to
    /// PascalCase for SARIF rule descriptor names. Single source of truth: we
    /// don't maintain our own CWE dictionary.
    /// </summary>
    public sealed partial class CweNameResolver : IDisposable
    {
        // Source-generated, non-backtracking, culture-invariant, explicit-capture-only.
        // MITRE pages use consistent casing so IgnoreCase is unnecessary.
        [GeneratedRegex(
            @"<title>CWE\s*-\s*CWE-\d+:\s*(?<name>.+?)\s*\(",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking)]
        private static partial Regex TitlePattern();

        [GeneratedRegex(
            @"<h2>CWE-\d+:\s*(?<name>.+?)</h2>",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking)]
        private static partial Regex H2Pattern();

        private readonly HttpClient _http;
        private readonly ConcurrentDictionary<string, string?> _cache = new(StringComparer.OrdinalIgnoreCase);

        public CweNameResolver()
        {
            this._http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            this._http.DefaultRequestHeaders.Add("User-Agent", "Sarif.Mcp.Server/1.0");
        }

        /// <summary>
        /// Resolves a CWE ID (e.g., "CWE-290") to a PascalCase name
        /// (e.g., "AuthenticationBypassBySpoofing"). Returns null if the ID is
        /// not a CWE or the fetch fails.
        /// </summary>
        public string? Resolve(string baseRuleId)
        {
            if (!baseRuleId.StartsWith("CWE-", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (this._cache.TryGetValue(baseRuleId, out string? cached))
            {
                return cached;
            }

            string? name = FetchFromMitre(baseRuleId).GetAwaiter().GetResult();
            this._cache[baseRuleId] = name;
            return name;
        }

        private async Task<string?> FetchFromMitre(string cweId)
        {
            try
            {
                string number = cweId[4..]; // "CWE-290" → "290"
                string url = $"https://cwe.mitre.org/data/definitions/{number}.html";
                string html = await this._http.GetStringAsync(url).ConfigureAwait(false);

                // Page title format: "CWE - CWE-290: Authentication Bypass by Spoofing (4.16)"
                Match titleMatch = TitlePattern().Match(html);
                if (titleMatch.Success)
                {
                    return ToPascalCase(titleMatch.Groups["name"].Value.Trim());
                }

                // Fallback: try the h2 heading.
                Match h2Match = H2Pattern().Match(html);
                if (h2Match.Success)
                {
                    return ToPascalCase(h2Match.Groups["name"].Value.Trim());
                }

                return null;
            }
            catch
            {
                // Network failure is not fatal — rule just won't have a name.
                return null;
            }
        }

        private static string ToPascalCase(string name)
        {
            string[] words = name.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..];
                }
            }

            return string.Concat(words);
        }

        public void Dispose() => this._http.Dispose();
    }
}
