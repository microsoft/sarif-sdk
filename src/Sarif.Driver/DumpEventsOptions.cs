// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Verb("dump-events", HelpText = "Export ETW events manifest file.")]
    public class DumpEventsOptions : CommonOptionsBase
    {
        [Value(0,
            MetaName = "<inputFile>",
            HelpText = "A path to a driver ETL log that should be dumped to the console.",
            Required = true)]
        public string EventsFilePath { get; set; }


        [Option(
        "csv",
        HelpText = "A file path to which all ETW events should be persisted as CSV.")]
        public string CsvFilePath { get; set; }

    }
}
