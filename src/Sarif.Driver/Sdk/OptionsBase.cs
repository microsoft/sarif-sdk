// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class OptionsBase
    {
        [Option(
            "rich-return-code",
            HelpText = "Emit a 'rich' return code consisting of a bitfield of conditions (as opposed to 0 or 1 indicating success or failure.")]
        public bool RichReturnCode { get; set; }
    }
}