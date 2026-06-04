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
    /// The invocation is supplied as a JSON document (file via <c>--input</c> or piped on stdin).
    /// Notifications travel inline on <c>toolExecutionNotifications</c> /
    /// <c>toolConfigurationNotifications</c>.
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
