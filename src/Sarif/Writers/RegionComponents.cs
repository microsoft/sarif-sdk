// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
