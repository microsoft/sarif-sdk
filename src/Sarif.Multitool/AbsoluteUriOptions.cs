// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("absoluteuri", HelpText = "Turn all relative Uris into absolute URIs (e.x. after rebaseUri is run)")]
    internal class AbsoluteUriOptions : MultitoolOptionsBase
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
    }
}
