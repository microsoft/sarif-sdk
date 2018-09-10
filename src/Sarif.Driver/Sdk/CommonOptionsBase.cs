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
        public bool PrettyPrint { get; internal set; }

        [Option(
            'f',
            "force",
            Default = false,
            HelpText = "Force overwrite of output file if it exists.")]
        public bool Force { get; internal set; }

        [Option(
            "insert",
            Separator = ';',
            HelpText =
            "Optionally present data, expressed as a semicolon-delimited list, that should be inserted into the log file. " +
            "Valid values include Hashes, TextFiles, BinaryFiles, EnvironmentVariables, CodeSnippets, SurroundingCodeSnippets " +
            "and NondeterministicProperties.")]
        public IEnumerable<OptionallyEmittedData> DataToInsert { get; internal set; }

        [Option(
            'u',
            "uriBaseIds",
            Separator = ';',
            HelpText =
            @"A key + value pair that defines a uriBaseId and its corresponding local file path. E.g., SRC=c:\src;TEST=c:\test")]
        public IEnumerable<string> UriBaseIds { get; internal set; }
    }
}