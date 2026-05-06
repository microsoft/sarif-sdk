// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("partition", HelpText = "Partition a single SARIF log into multiple logs according to a splitting strategy.")]
    public class PartitionOptions : SingleFileOptionsBase
    {
        [Option(
            "output-directory",
            HelpText = "The directory to write the partitioned SARIF logs to. Defaults to the current directory.")]
        public string OutputDirectoryPath { get; set; }

        [Option(
            "output-file-prefix",
            Default = "partition",
            HelpText = "Filename prefix for each output log. Files are named '{prefix}_{partitionKey}.sarif'.")]
        public string OutputFilePrefix { get; set; }

        [Option(
            "strategy",
            Default = SplittingStrategy.PerRule,
            HelpText = "Splitting strategy. Supported values: PerRule, PerRunPerRule, PerRunPerTarget, " +
                       "PerRunPerTargetPerRule, PerRun, PerResult, PerIndexList. " +
                       "PerIndexList requires --indices.")]
        public SplittingStrategy SplittingStrategy { get; set; }

        [Option(
            "indices",
            HelpText = "Required when --strategy=PerIndexList. A spec in the compact mini-language: " +
                       "'<runId>:<r1>,<r2>,...;<runId>:...|<bucket>|...'. " +
                       "The '<runId>:' prefix is optional and defaults to 0. " +
                       "SARIF URLs ('sarif:/runs/X/results/Y') are also accepted as segments. " +
                       "Buckets are separated by '|', segments within a bucket by ';', and indices within a segment by ','. " +
                       "Buckets are auto-named 'bucket0', 'bucket1', etc. Duplicate or out-of-range addresses cause an error.")]
        public string Indices { get; set; }

        [Option(
            "spillover-bucket",
            HelpText = "Optional bucket name (only meaningful with --strategy=PerIndexList). When set, any result not " +
                       "addressed by --indices is written to this bucket instead of being discarded.")]
        public string SpilloverBucket { get; set; }

        [Option(
            "strict-coverage",
            HelpText = "Only meaningful with --strategy=PerIndexList. When set, every result in the input log must be " +
                       "addressed by --indices (or covered by --spillover-bucket); otherwise the command fails.")]
        public bool StrictCoverage { get; set; }
    }
}
