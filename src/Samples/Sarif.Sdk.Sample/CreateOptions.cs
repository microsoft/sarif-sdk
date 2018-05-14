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
    }
}
