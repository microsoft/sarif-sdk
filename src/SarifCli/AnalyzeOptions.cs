// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Cli
{
    [Verb("validate", HelpText = "Validate a SARIF file against the schema and against additional correctness rules.")]
    internal class AnalyzeOptions : AnalyzeOptionsBase
    {
        [Option(
            'j',
            "json-schema",
            HelpText = "Path to the SARIF JSON schema.",
            Required = true)]
        public string SchemaFilePath { get; set; }
    }
}
