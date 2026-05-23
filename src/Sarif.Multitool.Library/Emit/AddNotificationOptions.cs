// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>add-notification</c>, which appends a fully-formed SARIF <c>notification</c>
    /// object to a staged event log (<c>&lt;output&gt;.wip.jsonl</c>) created by
    /// <c>emit-init-run</c>.
    /// </summary>
    /// <remarks>
    /// The notification is supplied as a JSON document (file via <c>--input</c> or piped on
    /// stdin). AI producers are expected to emit notifications with potentially very rich data
    /// — associated rule references, full exception trees, descriptive markdown messages,
    /// per-call properties — so the JSON-payload contract avoids encoding-by-flag entirely and
    /// preserves whatever the producer chose to express.
    /// </remarks>
    [Verb("add-notification", HelpText = "Append a fully-formed SARIF notification (JSON) to a staged event log.")]
    public class AddNotificationOptions
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
            HelpText = "Path to a JSON file containing the SARIF notification object. If omitted, JSON is read from stdin.")]
        public string InputFilePath { get; set; }
    }
}
