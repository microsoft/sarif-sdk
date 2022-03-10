// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("rewrite", HelpText = "Enrich a SARIF file with additional data.")]
    public class RewriteOptions : SingleFileOptionsBase
    {
        [Option(
            's',
            "sort-results",
            Default = false,
            HelpText = "Sort results in the final output file.")]
        public bool SortResults { get; set; }
    }
}
