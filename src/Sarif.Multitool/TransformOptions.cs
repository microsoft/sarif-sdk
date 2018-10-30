// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("transform", HelpText = "Transform a SARIF log to a different version.")]
    internal class TransformOptions : SingleFileOptionsBase
    {
        [Option(
            't',
            "target-version",
            HelpText = "The SARIF version to which the input file will be transformed.",
            Default = SarifVersion.Current)]
        public SarifVersion Version { get; internal set; }
    }
}