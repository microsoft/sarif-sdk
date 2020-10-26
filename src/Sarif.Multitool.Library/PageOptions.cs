// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    ///  Options for the 'Page' command, which quickly writes a subset of a SARIF file
    ///  for easier consumption of huge files.
    /// </summary>
    /// <remarks>
    ///  Excluded Options
    ///  ================
    ///    pretty-print: We copy slices of the input file, so we can't change formatting.
    ///    inline: We build a map of the input, so we don't want to write inline and immediately invalidate it.
    /// </remarks>
    [Verb("page", HelpText = "Extract a subset of results from a source SARIF file.")]
    public class PageOptions
    {
        [Value(0,
            MetaName = "<inputFile>",
            HelpText = "A path to a SARIF file to process.",
            Required = true)]
        public string InputFilePath { get; set; }

        [Option('r',
            "run-index",
            HelpText = "0-based index of the run to copy.",
            Required = false,
            Default = 0)]
        public int RunIndex { get; set; }

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
            'f',
            "force",
            Default = false,
            HelpText = "Force overwrite of output file if it exists.")]
        public bool Force { get; set; }

        [Option(
            'o',
            "output",
            HelpText = "A file path of the SARIF subset output file.",
            Required = true)]
        public string OutputFilePath { get; set; }

        [Option("map-ratio",
            HelpText = "Target map size relative to file size (0.01 is 1%)",
            Required = false,
            Default = 0.01)]
        public double TargetMapSizeRatio { get; set; } = 0.01;
    }
}
