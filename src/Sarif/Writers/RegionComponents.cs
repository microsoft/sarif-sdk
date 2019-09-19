// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    ///  RegionComponents has flags for the different ways a Region
    ///  can be identified.
    /// </summary>
    [Flags]
    public enum RegionComponents : short
    {
        None = 0,
        LineAndColumn = 1,
        ByteOffsetAndLength = 2,
        CharOffsetAndLength = 4,
        Full = short.MaxValue
    }
}
