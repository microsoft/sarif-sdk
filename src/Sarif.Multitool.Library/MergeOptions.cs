// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("merge", HelpText = "Merge multiple SARIF files into one.")]
    public class MergeOptions : MultipleFilesOptionsBase
    {
        [Option(
            "output-file",
            Default = "merged.sarif",
            HelpText = "File name to write merged content to.")]
        public string OutputFileName { get; internal set; }

        [Option(
            "merge-empty-logs",
            HelpText = "Merge log files with no results into the final log.")]
        public bool MergeEmptyLogs { get; internal set; }

        [Option(
            "split",
            HelpText = "Apply a splitting strategy to the merged file. " +
                       "Must be one of None or PerRule. By default ('None'), " + 
                       "no splitting strategy is applied (i.e. all input " + 
                       "files will be merged into a single log).",
            Default = SplittingStrategy.None)]
        public SplittingStrategy SplittingStrategy { get; internal set; }

        [Option(
            "merge-runs",
            HelpText = "Merge runs of the same tool + verion combination (requires " + 
                       "eliding run-specific details such as invocations data.")]
        public bool MergeRuns { get; internal set; }
    }
}
