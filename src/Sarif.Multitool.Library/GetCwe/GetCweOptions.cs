// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Output format for <c>get-cwe</c>.
    /// </summary>
    public enum CweOutputFormat
    {
        Json,
        Markdown,
    }

    /// <summary>
    /// Options for <c>get-cwe</c>, which serves canonical MITRE CWE data from the SDK's embedded
    /// taxonomy. Each record carries a <c>ruleIdFallback</c> — the kebab-cased
    /// <c>CWE-&lt;n&gt;/&lt;slug&gt;</c> a producer can drop into <c>result.ruleId</c> when it will
    /// not author a sharper sub-id. The fallback is computed the same way AI1012 derives its
    /// suggestion, so the two always agree.
    /// </summary>
    [Verb("get-cwe", HelpText = "Emit canonical MITRE CWE data (id, name, slug, ruleId fallback, status, help). Pass CWE ids or --all.")]
    public class GetCweOptions
    {
        [Value(
            0,
            MetaName = "<ids>",
            HelpText = "Comma-separated CWE ids to emit (e.g., '89', 'CWE-89', '79,89'). Mutually exclusive with --all.",
            Required = false)]
        public string Ids { get; set; }

        [Option(
            "all",
            HelpText = "Emit every entry in the embedded catalog, including deprecated and obsolete CWEs. Mutually exclusive with <ids>.",
            Default = false)]
        public bool All { get; set; }

        [Option(
            "concise",
            HelpText = "Omit the full MITRE help markdown from each record, emitting only the high-level fields.",
            Default = false)]
        public bool Concise { get; set; }

        [Option(
            'f',
            "format",
            HelpText = "Output format: 'json' (default) or 'markdown'.",
            Default = CweOutputFormat.Json)]
        public CweOutputFormat Format { get; set; }

        [Option(
            'o',
            "output",
            HelpText = "Path to write the output to. If omitted, the output is written to stdout.")]
        public string OutputFilePath { get; set; }

        [Option(
            "force-overwrite",
            HelpText = "Overwrite the --output file if it already exists.",
            Default = false)]
        public bool ForceOverwrite { get; set; }
    }
}
