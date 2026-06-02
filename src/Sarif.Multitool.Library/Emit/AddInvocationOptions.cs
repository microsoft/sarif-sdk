// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>add-invocation</c>, which appends a fully-formed SARIF <c>invocation</c>
    /// object to a staged event log (<c>&lt;output&gt;.wip.jsonl</c>) created by
    /// <c>emit-init-run</c>.
    /// </summary>
    /// <remarks>
    /// The invocation is supplied as a JSON document (file via <c>--input</c> or piped on
    /// stdin). <see cref="SarifEventReplayer"/> strips any <c>invocations</c> array carried on
    /// the run header — invocations must arrive as their own events — so this verb is the
    /// only path a producer has to populate <c>run.invocations[]</c>. Each invocation is a
    /// complete, self-contained subtree: the producer supplies any notifications INLINE on the
    /// payload's <c>toolExecutionNotifications</c> / <c>toolConfigurationNotifications</c> arrays
    /// and emits one such invocation per launched process (including parallel/overlapping
    /// processes, each modeled by its own invocation event).
    /// </remarks>
    [Verb("add-invocation", HelpText = "Append a fully-formed SARIF invocation (JSON) to a staged event log.")]
    public class AddInvocationOptions
    {
        [Value(
            0,
            MetaName = "<outputSarifPath>",
            HelpText = "Path to the final SARIF file; the event log is appended to '<output>.wip.jsonl'.",
            Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            'i',
            "input",
            HelpText = "Path to a JSON file containing the SARIF invocation object. If omitted, JSON is read from stdin.")]
        public string InputFilePath { get; set; }
    }
}
