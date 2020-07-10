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
    }
}
