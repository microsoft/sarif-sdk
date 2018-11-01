// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    [Flags]
    public enum DictionaryMergeBehaviors
    {
        None = 0,                      // By default, we will always exclusively  preserve the earliest
                                       // properties that have been generated. We will not attempt any merging.
        InitializeFromCurrent = 0x1,   // On setting this bit, we will discard earlier properties
                                       // in favor of those that are most recently generated.
        Merge = 0x2,                   // Merging will allow properties from previous and current to
                                       // be merged together. Collisions will raise an exception
        ForceMerge = 0x4               // Merge without raising exception on collisions. If a shared
                                       // key is encountered, the merge will favor previous or current
                                       // based on whether the 'InitializeFromCurrent' bit is set
    }
}
