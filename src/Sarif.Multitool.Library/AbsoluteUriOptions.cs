// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("absoluteuri", HelpText = "Turn all relative Uris into absolute URIs (to be used after rebaseUri is run)")]
    public class AbsoluteUriOptions : MultipleFilesOptionsBase
    {

    }
}
