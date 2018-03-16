// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("rebaseuri", HelpText = "Rebase the URIs in one or more sarif files.")]
    internal class RebaseUriOptions : MultitoolOptionsBase
    {
        [Value(0,
            MetaName = "<files>",
            HelpText = "Files to process (wildcards ? and * allowed).",
            Required = false)]
        public IList<string> Files { get; internal set; }

        [Option(
            'r',
            "recurse",
            Default = false,
            HelpText = "Recursively select subdirectories in paths.")]
        public bool Recurse { get; internal set; }

        [Option(
            'b',
            "base-path-value",
            Required = true,
            HelpText = "Base path value to use while rebasing all paths.  E.x. 'C:\\bld\\1234\\bin\\'"
            )]
        public string BasePath { get; internal set; }

        [Option(
            'n',
            "base-path-name",
            Required = true,
            HelpText = "Variable to use for the base path (e.x. 'SRCROOT')"
            )]
        public string BasePathName { get; internal set; }
    }
}
