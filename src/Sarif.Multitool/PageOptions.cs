// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("page", HelpText = "Extract a subset of results from a source SARIF file.")]
    internal class PageOptions
    {
        [Value(0,
            MetaName = "<inputFile>",
            HelpText = "A path to a SARIF file to process.",
            Required = true)]
        public string InputFilePath { get; set; }

        [Option('i',
            "index",
            HelpText = "0-based index of first result to include.",
            Required = true)]
        public int Index { get; set; }

        [Option('c',
            "count",
            HelpText = "Number of results from index to include.",
            Required = true)]
        public int Count { get; set; }

        [Option(
            'o',
            "output",
            HelpText = "A file path of the SARIF subset output file.",
            Required = true)]
        public string OutputFilePath { get; set; }
    }
}
