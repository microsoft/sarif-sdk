// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Verb("export-rule-documentation", HelpText = "Export the documentation for the validation rules to a Markdown file.")]
    public class ExportRuleDocumentationOptions
    {
        [Option(
               'o',
               "output-file-path",
               HelpText = "Path to the generated Markdown file. Default: ValidationRules.md in the current directory.")]
        public string OutputFilePath { get; internal set; }
    }
}
