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

        [Option(
            "normalize-for-ghas",
            HelpText = "Normalize SARIF to conform to GitHub Advanced Security (GHAS) code scanning ingestion requirements.")]
        public bool NormalizeForGhas { get; set; }

        [Option(
            'b',
            "base-path-value",
            Required = false,
            HelpText = "Base path value to use while rebasing all paths.  E.x. 'C:\\bld\\1234\\bin\\'"
            )]
        public string BasePath { get; set; }

        [Option(
            't',
            "base-path-token",
            Required = false,
            HelpText = "Variable to use for the base path token (e.x. 'SRCROOT')"
            )]
        public string BasePathToken { get; set; }

        [Option(
            "rebase-relative-uris",
            Default = false,
            HelpText = "All relative uris will be rebased to the base-path-value if true. Default is false."
            )]
        public bool RebaseRelativeUris { get; set; }
    }
}
