// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Verb("export-config", HelpText = "Export rule options to an XML or JSON file that can be edited and used to configure subsequent analysis.")]
    public class ExportConfigurationOptions
    {
        [Value(0, HelpText = "Output path for exported analysis options", Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            'f',
            "format",
            Default = FileFormat.Json,
            HelpText = "The file format to persist settings to (if the format cannot be inferred from the file name extension).")]
        public FileFormat FileFormat { get; set; }

        [Option(
            "plugin",
            Separator = ';',
            HelpText = "Plugin paths, expressed as a semicolon-delimited list enclosed in double quotes, that " +
                       "will be invoked to retrieve rule options.")]
        public IEnumerable<string> PluginFilePaths { get; set; }
    }
}
