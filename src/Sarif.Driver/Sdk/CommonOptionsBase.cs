// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class CommonOptionsBase
    {
        [Option(
            'p',
            "pretty-print",
            Default = false,
            HelpText = "Produce pretty-printed JSON output rather than compact form.")]
        public bool PrettyPrint { get; set; }

        [Option(
            'f',
            "force",
            Default = false,
            HelpText = "Force overwrite of output file if it exists.")]
        public bool Force { get; set; }

        [Option(
            "insert",
            Separator = ';',
            HelpText =
            "Optionally present data, expressed as a semicolon-delimited list, that should be inserted into the log file. " +
            "Valid values include Hashes, TextFiles, BinaryFiles, EnvironmentVariables, RegionSnippets, ContextRegionSnippets, " +
            "Guids and NondeterministicProperties.")]
        public IEnumerable<OptionallyEmittedData> DataToInsert { get; set; }

        [Option(
            "remove",
            Separator = ';',
            HelpText =
            "Optionally present data, expressed as a semicolon-delimited list, that should be not be persisted to or which " +
            "should be removed from the log file. Valid values include Hashes, TextFiles, BinaryFiles, EnvironmentVariables, " +
            "RegionSnippets, ContextRegionSnippets and NondeterministicProperties.")]
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
            @"The SARIF version of the output log file. Valid values are OneZeroZero and Current",
            Default = SarifVersion.Current)]
        public SarifVersion SarifOutputVersion { get; set; }
    }
}