// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class SingleFileOptionsBase : MultitoolOptionsBase
    {
        [Value(0,
            MetaName = "<inputFile>",
            HelpText = "A path to a file to process.",
            Required = true)]
        public string InputFilePath { get; internal set; }
        
        [Option(
            'i',
            "inline",
            Default = false,
            HelpText = "Write all transformed content to the input file.")]
        public bool Inline { get; internal set; }
    }
}
