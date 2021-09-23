// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Sarif.Sdk.Sample
{
    [Verb("create", HelpText = "Creates a SARIF log file using the SarifLogger and writes it disk.")]
    internal class CreateOptions
    {
        [Value(1,
               MetaName = "<outputFile>",
               HelpText = "The path of the file to be written.",
               Required = true)]
        public string OutputFilePath { get; internal set; }

        [Option("numResult",
                Default = 1,
                HelpText = "Number of results per rule to be generated in the sample program",
                Required = false)]
        public int NumOfResultPerRule { get; set; }

        [Option("useFileStream",
        Default = false,
        HelpText = "Use file stream to write sarif file if true.",
        Required = false)]
        public bool UseFileStream { get; set; }
    }
}
