// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>get-schema</c>, which emits the JSON Schema that validates the input to a
    /// named emit verb. The schema is written verbatim to stdout, or to <c>--output</c>.
    /// </summary>
    /// <remarks>
    /// The schemas served here are the same bytes the emit verbs validate their inputs against,
    /// so a producer can fetch the contract for the exact verb it is about to call.
    /// </remarks>
    [Verb("get-schema", HelpText = "Emit the JSON Schema for an emit verb's input. Pass --list to enumerate the verbs that have a schema.")]
    public class GetSchemaOptions
    {
        [Value(
            0,
            MetaName = "<verb>",
            HelpText = "The emit verb whose input schema to emit (e.g., 'emit-run', 'emit-results').",
            Required = false)]
        public string Verb { get; set; }

        [Option(
            'o',
            "output",
            HelpText = "Path to write the schema to. If omitted, the schema is written to stdout.")]
        public string OutputFilePath { get; set; }

        [Option(
            "list",
            HelpText = "List the verbs that have a schema, then exit.",
            Default = false)]
        public bool List { get; set; }

        [Option(
            "force-overwrite",
            HelpText = "Overwrite the --output file if it already exists.",
            Default = false)]
        public bool ForceOverwrite { get; set; }
    }
}
