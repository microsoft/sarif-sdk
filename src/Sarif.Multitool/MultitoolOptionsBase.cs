// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool{
    public class MultitoolOptionsBase
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
            'h',
            "hashes",
            HelpText = "Output MD5, SHA1, and SHA-256 hash of analysis targets when emitting SARIF reports.")]
        public bool ComputeFileHashes { get; set; }

        [Option(
            't', // persist 't'ext
            "persist-text-contents",
            HelpText = "Persist a base64-encoded representation of all referenced textual files to the log.")]
        public bool PersistTextFileContents { get; set; }

        [Option(
            // Lack of shortcut for this one is by design, as the need
            // to persist this data is unusual and could lead to insecurities.
            // It is therefore helpful for the command to be very readable
            // in all usages.
            "persist-binary-contents",
            HelpText = "Persist a base64-encoded representation of all referenced files that are presumed to be non-textual to the log.")]
        public bool PersistBinaryContents { get; set; }
    }
}