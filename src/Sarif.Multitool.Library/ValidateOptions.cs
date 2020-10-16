// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("validate", HelpText = "Validate a SARIF file against the schema and against additional correctness rules.")]
    public class ValidateOptions : AnalyzeOptionsBase
    {
        [Option(
            'j',
            "json-schema",
            HelpText = "Path to the SARIF JSON schema.")]
        public string SchemaFilePath { get; set; }

        [Option(
            "update-inputs-to-current-sarif",
            HelpText =
            @"Update any SARIF v1 or prerelease v2 files to the current SARIF v2 format.")]
        public bool UpdateInputsToCurrentSarif { get; set; }
    }
}
