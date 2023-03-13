// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Flags]
    public enum DefaultTraces
    {
        None,
        /// <summary>
        /// Enables a trace message that reports the elapsed time for the end-to-end scan.
        /// </summary>
        ScanTime = 0x01,
        /// <summary>
        /// Enables a trace message that reports the elapsed time for every rule, per scan target.
        /// </summary>
        RuleScanTime = 0x2,
        /// <summary>
        /// Enables a trace message that reports peak working set during analysis.
        /// </summary>
        PeakWorkingSet = 0x4,
        /// <summary>
        /// Enables a trace message that reports progress against each scan target.
        /// </summary>
        TargetsScanned = 0x8,
    }
}
