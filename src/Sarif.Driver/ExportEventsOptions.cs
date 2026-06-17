// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Verb("export-events", HelpText = "Export ETW events manifest file.")]
    public class ExportEventsOptions : CommonOptionsBase
    {
        [Value(0,
            MetaName = "<outputFile>",
            HelpText = "A path to which the events manifest file should be written.",
            Required = true)]
        public string OutputFilePath { get; set; }
    }
}
