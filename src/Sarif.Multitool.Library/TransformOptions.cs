// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("transform", HelpText = "Transform a SARIF log to a different version.")]
    public class TransformOptions : SingleFileOptionsBase
    {
    }
}