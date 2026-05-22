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
    }
}
