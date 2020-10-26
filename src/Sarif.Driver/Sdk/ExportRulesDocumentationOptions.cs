﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Verb("export-docs", HelpText = "Export the documentation for the analysis rules to a Markdown file.")]
    public class ExportRulesDocumentationOptions
    {
        [Option(
               'o',
               "output-file-path",
               HelpText = "Path to the generated Markdown file. Default: Rules.md in the current directory.")]
        public string OutputFilePath { get; internal set; }
    }
}
