// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>emit-init-run</c>, which opens an append-only event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) seeded with a <c>run-header</c> event built from a
    /// caller-supplied SARIF <c>Run</c> JSON document. Subsequent producers append events to the
    /// log via the SARIF emit API and finalize via <c>multitool emit-finalize</c>.
    /// </summary>
    /// <remarks>
    /// <para>The run JSON is supplied as a JSON document (file via <c>--input</c> or piped on
    /// stdin) and may contain any partial-<c>Run</c> fields the replayer accepts.</para>
    /// <para>Profile-essential defects are validated at receipt: required <c>tool.driver.name</c>,
    /// URI schemes, canonical GUIDs, <c>properties["ai/origin"]</c>, and accidental SARIF-log input.</para>
    /// </remarks>
    [Verb("emit-init-run", HelpText = "Open an append-only event log seeded with a SARIF run header (JSON).")]
    public class EmitInitRunOptions
    {
        [Value(
            0,
            MetaName = "<outputSarifPath>",
            HelpText = "Path to the final SARIF file (the event log is staged alongside as '<output>.wip.jsonl').",
            Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            'i',
            "input",
            HelpText = "Path to a JSON file containing the SARIF run object. If omitted, JSON is read from stdin.")]
        public string InputFilePath { get; set; }

        [Option(
            "force-overwrite",
            HelpText = "Force replacement of an existing .sarif or in-progress .wip.jsonl at the destination.",
            Default = false)]
        public bool ForceOverwrite { get; set; }
    }
}
