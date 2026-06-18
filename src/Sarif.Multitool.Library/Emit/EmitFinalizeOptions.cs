// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>emit-finalize</c>, which replays the staged event log and atomically
    /// writes the destination SARIF file.
    /// </summary>
    [Verb("emit-finalize", HelpText = "Replay a staged SARIF event log into a final SARIF file.")]
    public class EmitFinalizeOptions
    {
        [Value(
            0,
            MetaName = "<outputSarifPath>",
            HelpText = "Path to the final SARIF file; the event log is read from '<output>.wip.jsonl'.",
            Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            "no-cwe-enrichment",
            HelpText = "Skip enrichment of CWE-as-rule-id descriptors from the embedded MITRE CWE taxonomy.",
            Default = false)]
        public bool NoCweEnrichment { get; set; }

        [Option(
            "keep-wip",
            HelpText = "Retain the '<output>.wip.jsonl' event log after successful finalize. Useful for forensics and reruns.",
            Default = false)]
        public bool KeepWip { get; set; }

        [Option(
            "minify",
            HelpText = "Produce compact (single-line) JSON rather than indented output.",
            Default = false)]
        public bool Minify { get; set; }

        [Option(
            "embed-text-files",
            HelpText = "Embed the textual content of every text-file artifact referenced by the run (run.artifacts[].contents.text). Use for self-contained AI fixtures and to clear SARIF2013.",
            Default = false)]
        public bool EmbedTextFiles { get; set; }

        [Option(
            "validate",
            HelpText = "After writing the SARIF, run the multitool validator (--rule-kind Sarif;AI) against the output. Fails with non-zero exit and prints a summary if any Error-level findings are reported. Warnings/Notes are summarized but do not fail.",
            Default = false)]
        public bool Validate { get; set; }

        [Option(
            "no-repo",
            HelpText = "Finalize a scan that has no version control (a local working copy, an unpacked container image, or a downloaded package). Tolerates absent run.versionControlProvenance, elides any transient local file:// source-root base from the output, and marks each run unpublishable. An unpublishable run cannot be uploaded to a code-scanning alert store, which anchors every alert to a repository and commit.",
            Default = false)]
        public bool NoRepo { get; set; }
    }
}
