// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    /// <summary>
    /// Result metadata for matching a group of results from a 
    /// set of runs (such as the current set of results) to a group of results 
    /// from a different set of runs (such as the prior set of results).
    /// </summary>
    public class ExtractedResult
    {
        public Result Result { get; set; }
        
        public Run OriginalRun { get; set; }
    }
}
