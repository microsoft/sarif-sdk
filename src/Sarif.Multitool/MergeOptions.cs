// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("merge", HelpText = "Merge multiple SARIF files into one.")]
    internal class MergeOptions : MultitoolOptionsBase
    {
        [Value(0,
            MetaName = "<files>",
            HelpText = "Files to process (wildcards ? and * allowed).",
            Required = false)]
        public IList<string> Files { get; internal set; }
    }
}
