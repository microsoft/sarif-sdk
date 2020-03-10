// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    [Flags]
    public enum DictionaryMergeBehavior
    {
        None = 0,
        InitializeFromOldest = 0x1,    // On setting this bit, we retain oldest property bags for matched items
        InitializeFromMostRecent = 0x2,// On setting this bit, we will discard earlier properties
                                       // in favor of those that are most recently generated.
    }
}
