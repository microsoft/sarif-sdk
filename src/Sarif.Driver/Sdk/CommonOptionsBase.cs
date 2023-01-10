// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using CommandLine;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class CommonOptionsBase
    {
        [Option(
            'p',
            "pretty-print",
            Default = false,
            HelpText =
            "Produce pretty-printed JSON output rather than compact output (all white space removed). If neither " +
            "--pretty-print nor --minify is specified, --pretty-print is set to true. --pretty-print and --minify " +
            "cannot be specified together.")]
        public bool PrettyPrint { get; set; }

        [Option(
            'm',
            "minify",
            Default = false,
            HelpText = "Produce compact JSON output (all white space removed) rather than pretty-printed output. " +
            "If neither --pretty-print nor --minify is specified, --pretty-print is set to true. --pretty-print " +
            "and --minify cannot be specified together.")]
        public bool Minify { get; set; }

        [Option(
            'f',
            "force",
            Default = false,
            HelpText = "Force overwrite of output file if it exists.")]
        public bool Force { get; set; }

        [Option(
            'i',
            "inline",
            Default = false,
            HelpText = "Overwrite each input file with the corresponding transformed file.")]
        public bool Inline { get; set; }

        [Option(
            "insert",
            Separator = ';',
            HelpText =
            "Optionally present data, expressed as a semicolon-delimited list, that should be inserted into the log file. " +
            "Valid values include Hashes, TextFiles, BinaryFiles, EnvironmentVariables, RegionSnippets, ContextRegionSnippets, " +
            "ContextRegionSnippetPartialFingerprints, Guids, VersionControlDetails, and NondeterministicProperties.")]
        public IEnumerable<OptionallyEmittedData> DataToInsert { get; set; }

        [Option(
            "remove",
            Separator = ';',
            HelpText =
            "Optionally present data, expressed as a semicolon-delimited list, that should be not be persisted to or which " +
            "should be removed from the log file. Valid values include Hashes, TextFiles, BinaryFiles, EnvironmentVariables, " +
            "RegionSnippets, ContextRegionSnippets, Guids, VersionControlDetails, and NondeterministicProperties.")]
        public IEnumerable<OptionallyEmittedData> DataToRemove { get; set; }

        [Option(
            'u',
            "uriBaseIds",
            Separator = ';',
            HelpText =
            @"A key + value pair that defines a uriBaseId and its corresponding local file path. E.g., SRC=c:\src;TEST=c:\test")]
        public IEnumerable<string> UriBaseIds { get; set; }

        [Option(
            'v',
            "sarif-output-version",
            HelpText =
            "The SARIF version of the output log file. Valid values are OneZeroZero and Current",
            Default = SarifVersion.Current)]
        public SarifVersion SarifOutputVersion { get; set; } = SarifVersion.Current;

        [Option(
            "threads",
            HelpText = "A count of threads that should be used for multithreaded operations.")]
        public int Threads { get; set; }

        [Option(
            "insert-property",
            Separator = ';',
            HelpText =
            "A semicolon-delimited list of JSON path + property values that should be inserted into the output log. Currently, " +
            "only paths that point to a version control provenance property bag is supported, e.g., 'runs[0].invocations[1]." +
            "versionControlProvenance.properties.myProperty=myValue'.")]
        public IEnumerable<string> InsertProperties { get; set; }

        [Option(
            "automationId",
            HelpText = "An id that will be persisted to the 'Run.AutomationDetails.Id' property. See section '3.17.3' of the SARIF specification for more information.")]
        public string AutomationId { get; set; }

        [Option(
            "automationGuid",
            HelpText = "A guid that will be persisted to the 'Run.AutomationDetails.Guid' property. See section '3.17.4' of the SARIF specification for more information.")]
        public Guid? AutomationGuid { get; set; }

        public Formatting Formatting => this.PrettyPrint || (!this.PrettyPrint && !this.Minify)
            ? Formatting.Indented
            : Formatting.None;
    }
}
