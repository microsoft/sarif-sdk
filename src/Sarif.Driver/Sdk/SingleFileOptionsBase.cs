// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class SingleFileOptionsBase : CommonOptionsBase
    {
        [Value(0,
            MetaName = "<inputFile>",
            HelpText = "A path to a file to process.",
            Required = true)]
        public string InputFilePath { get; set; }

        [Option(
            'i',
            "inline",
            Default = false,
            HelpText = "Write all newly generated content to the input file.")]
        public bool Inline { get; set; }

        [Option(
            'o',
            "output",
            HelpText = "A file path to the generated SARIF log.")]
        public string OutputFilePath { get; set; }
    }
}
