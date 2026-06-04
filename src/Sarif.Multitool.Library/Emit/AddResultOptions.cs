// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>add-result</c>, which appends a fully-formed SARIF <c>result</c> object
    /// to a staged event log (<c>&lt;output&gt;.wip.jsonl</c>) created by <c>emit-run</c>.
    /// </summary>
    /// <remarks>
    /// The result is supplied as a JSON document (file via <c>--input</c> or piped on stdin).
    /// On receipt the verb validates <c>result.ruleId</c> against the AI ruleId convention.
    /// </remarks>
    [Verb("add-result", HelpText = "Append a fully-formed SARIF result (JSON) to a staged event log.")]
    public class AddResultOptions
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
            HelpText = "Path to a JSON file containing the SARIF result object. If omitted, JSON is read from stdin.")]
        public string InputFilePath { get; set; }
    }
}
