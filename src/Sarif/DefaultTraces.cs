// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Flags]
    public enum DefaultTraces
    {
        None,
        ScanTime = 0x01,
        ScanExecution = 0x2,
        RuleScanTime = 0x04,
    }
}
