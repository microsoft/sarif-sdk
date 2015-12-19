// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    [Verb("exportConfig", HelpText = "Export rule options to an XML file that can be edited and used to configure subsequent analysis.")]
    public class ExportConfigurationOptions
    {
        [Value(0, HelpText = "Output path for exported analysis options", Required = true)]
        public string OutputFilePath { get; set; }
    }
}
