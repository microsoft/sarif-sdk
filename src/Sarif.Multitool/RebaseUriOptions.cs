// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("rebaseuri", HelpText = "Rebase the URIs in one or more sarif files.")]
    public class RebaseUriOptions : MultipleFilesOptionsBase
    {
        [Option(
            'b',
            "base-path-value",
            Required = true,
            HelpText = "Base path value to use while rebasing all paths.  E.x. 'C:\\bld\\1234\\bin\\'"
            )]
        public string BasePath { get; internal set; }

        [Option(
            't',
            "base-path-token",
            Required = true,
            HelpText = "Variable to use for the base path token (e.x. 'SRCROOT')"
            )]
        public string BasePathToken { get; internal set; }

        [Option(
            "rebase-relative-uris",
            Default = false,
            HelpText = "All relative uris will be rebased to the base-path-value if true. Default is false."
            )]
        public bool RebaseRelativeUris { get; internal set; }
    }
}
