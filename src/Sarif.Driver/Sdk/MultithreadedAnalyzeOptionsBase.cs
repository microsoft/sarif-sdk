// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Verb("analyze", HelpText = "Analyze one or more binary files for security and correctness issues.")]
    public abstract class MultithreadedAnalyzeOptionsBase : AnalyzeOptionsBase
    {
        [Option(
            't',
            "threads",
            HelpText = "Maximum # of threads to use during analysis.")]
        public int ThreadCount { get; set; }
    }
}
