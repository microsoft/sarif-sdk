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
    /// stdin), matching the contract used by <c>add-result</c>, <c>add-invocation</c>, and
    /// <c>add-reporting-descriptor</c>. SARIF <c>Run</c> is by far the richest object in the
    /// schema; modeling each field as a CLI flag would require a sprawling and ever-expanding
    /// surface that still could not express the legal partial-<c>Run</c> shape the replayer
    /// accepts (multiple <c>versionControlProvenance</c> entries, <c>properties</c> bags,
    /// <c>language</c>, <c>columnKind</c>, <c>defaultEncoding</c>, <c>redactionTokens</c>, …).
    /// The JSON-payload contract keeps the verb generic and lets an AI producer emit
    /// arbitrarily-rich run headers without losing fidelity.</para>
    /// <para>Profile-essential defects are validated at receipt: <c>tool.driver.name</c> must
    /// be a non-empty string; <c>tool.driver.informationUri</c> and
    /// <c>versionControlProvenance[*].repositoryUri</c> must be <c>https</c>;
    /// <c>originalUriBaseIds["SRCROOT"].uri</c> must be <c>https</c> or <c>file</c>;
    /// <c>automationDetails.guid</c> / <c>correlationGuid</c> must be canonical 8-4-4-4-12
    /// GUIDs; <c>properties["ai/origin"]</c> must be <c>generated</c>, <c>annotated</c>, or
    /// <c>synthesized</c>. The verb also rejects a SARIF <em>log</em> accidentally supplied in
    /// place of a <c>Run</c>.</para>
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
