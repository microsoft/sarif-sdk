// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("rewrite", HelpText = "Transform a SARIF file to a reformatted version.")]
    public class RewriteOptions : SingleFileOptionsBase
    {

    }
}