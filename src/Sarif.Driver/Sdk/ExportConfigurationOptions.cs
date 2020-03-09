// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Verb("exportConfig", HelpText = "Export rule options to an XML or JSON file that can be edited and used to configure subsequent analysis.")]
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
    }
}
