﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class MultipleFilesOptionsBase : CommonOptionsBase
    {
        [Option(
            'r',
            "recurse",
            Default = false,
            HelpText = "Recursively select subdirectories in paths.")]
        public bool Recurse { get; set; }

        [Option(
            'o',
            "output-directory",
            HelpText = "A directory to output the transformed files to. If absent, each transformed file is written to the same directory as the corresponding input file.")]
        public string OutputDirectoryPath { get; set; }

        [Value(0,
            MetaName = "<files>",
            HelpText = "Files to process (wildcards ? and * allowed).",
            Required = true)]
        public IEnumerable<string> TargetFileSpecifiers { get; set; }
    }
}
