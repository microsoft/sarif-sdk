// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Verb("export-rules", HelpText = "Export rules metadata to a SARIF or SonarQube XML file.")]
    public class ExportRulesMetadataOptions
    {
        [Value(0, HelpText = "Output path for exported analysis options.\r\n" +
            "Use a .json or .sarif extension to produce SARIF.\r\n" +
            "Use .xml to produce a SonarQube rule descriptor file.\r\n" +
            "Use .md to produce a markdow rule descriptor file.", Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            "plug-in",
            Separator = ';',
            HelpText = "Path to plug-in that will be invoked to retrieve rule metadata.")]
        public IEnumerable<string> PluginFilePaths { get; set; }
    }
}
