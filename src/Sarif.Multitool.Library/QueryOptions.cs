// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    ///  Options for the 'Query' command, which runs a query expression on a SARIF file
    ///  and shows results.
    /// </summary>
    [Verb("query", HelpText = "Find the matching subset of a SARIF file and output it or log it.")]
    public class QueryOptions
    {
        [Value(0,
            MetaName = "<inputFile>",
            HelpText = "A path to a SARIF file to process.",
            Required = true)]
        public string InputFilePath { get; set; }

        [Option(
            'e',
            "expression",
            HelpText = "Result Expression to Evaluate (ex: (BaselineState != 'Unchanged'))",
            Required = true)]
        public string Expression { get; set; }

        [Option(
            'w',
            "write-to-console",
            Default = false,
            HelpText = "Whether to write matching results to the console")]
        public bool WriteToConsole { get; set; } = false;

        [Option(
            "non-zero-exit-code-if-count-over",
            Default = -1,
            HelpText = "Whether to return a non-zero exit code if the count exceeds a threshold")]
        public int NonZeroExitCodeIfCountOver { get; set; }

        [Option(
            'c',
            "return-count",
            Default = false,
            HelpText = "Exit Code is the count of matching results")]
        public bool ReturnCount { get; set; } = false;

        [Option(
            'f',
            "force",
            Default = false,
            HelpText = "Force overwrite of output file if it exists.")]
        public bool Force { get; set; } = false;

        [Option(
            'p',
            "pretty-print",
            Default = false,
            HelpText = "Produce pretty-printed JSON output rather than compact form.")]
        public bool PrettyPrint { get; set; }

        [Option(
            'o',
            "output",
            HelpText = "A file path of the SARIF subset output file, if desired.",
            Required = false)]
        public string OutputFilePath { get; set; }

    }
}
