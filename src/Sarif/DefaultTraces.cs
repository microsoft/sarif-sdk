// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Flags]
    public enum DefaultTraces
    {
        None,
        ScanTime = 0x1,
        MemoryUsage = 0x2,
    }
}
