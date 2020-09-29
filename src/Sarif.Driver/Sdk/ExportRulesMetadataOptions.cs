﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Verb("export-rules", HelpText = "Export rules metadata to a SARIF or SonarQube XML file.")]
    public class ExportRulesMetadataOptions
    {
        [Value(0, HelpText = "Output path for exported analysis options. Use a .json or .sarif extension to produce SARIF. Use .xml to produce a SonarQube rule descriptor file.", Required = true)]
        public string OutputFilePath { get; set; }
    }
}
