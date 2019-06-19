// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("result-matching-test", HelpText = "Match Results across runs in the same Group in a folder.")]
    internal class ResultMatchingTestOptions : CommonOptionsBase
    {
        [Value(0,
            MetaName = "folder-path",
            HelpText = "Path to a folder containing groups of logs to compare. File names are [Group]-[Run]",
            Required = true)]
        public string FolderPath { get; internal set; }


        [Option('o', 
            "output-folder-path", 
            HelpText = "Folder Path where output will be written.")]
        public string OutputFolderPath { get; internal set; }
    }
}
