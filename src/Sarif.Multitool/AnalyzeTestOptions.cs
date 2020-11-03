// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("analyze-test", HelpText = "Test the analysis driver framework.")]
    public class AnalyzeTestOptions : MultithreadedAnalyzeOptionsBase
    {
    }
}
