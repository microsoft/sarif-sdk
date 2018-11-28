// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    public enum DictionaryMergeBehavior
    {
        None = 0,                      // By default, we will always exclusively  preserve the earliest
                                       // properties that have been generated. We will not attempt any merging.
        InitializeFromOldest = None,
        InitializeFromMostRecent = 0x1,// On setting this bit, we will discard earlier properties
                                       // in favor of those that are most recently generated.
    }
}
