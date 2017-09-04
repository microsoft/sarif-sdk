// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Verb("rewrite", HelpText = "Transform a SARIF file to a reformatted version.")]
    internal class RewriteOptions : MultitoolOptionsBase
    {
    }
}