// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Shared options for the emit verbs that append a JSON object to a staged event log: the
    /// destination SARIF path and the JSON input (file or stdin).
    /// </summary>
    public abstract class EmitInputOptionsBase
    {
        [Value(
            0,
            MetaName = "<outputSarifPath>",
            HelpText = "Path to the final SARIF file; the event log is staged alongside as '<output>.wip.jsonl'.",
            Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            'i',
            "input",
            HelpText = "Path to a JSON file containing the input object. If omitted, JSON is read from stdin.")]
        public string InputFilePath { get; set; }
    }
}
