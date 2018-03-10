// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool{
    public class MultitoolOptionsBase
    {
        [Option(
            'o',
            "output",
            HelpText = "A file path to the generated SARIF log. Defaults to <input file name>.sarif.")]
        public string OutputFilePath { get; internal set; }

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
            'h',
            "hashes",
            HelpText = "Output MD5, SHA1, and SHA-256 hash of analysis targets when emitting SARIF reports.")]
        public bool ComputeFileHashes { get; set; }


        [Option(
            "persist-file-contents",
            HelpText = "Persist a base64-encoded representation of all referenced files to the log.")]
        public bool PersistFileContents { get; set; }
    }
}