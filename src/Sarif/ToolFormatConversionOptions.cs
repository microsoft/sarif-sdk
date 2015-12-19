// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>A bitfield of flags for specifying tool format conversion options.</summary>
    [Flags]
    public enum ToolFormatConversionOptions
    {
        /// <summary>No conversion options are selected.</summary>
        None = 0x0,

        /// <summary>If present, output logs shall be overwritten.</summary>
        OverwriteExistingOutputFile = 0x1,

        /// <summary>If present, output logs shall be pretty printed.</summary>
        PrettyPrint = 0x2
    }
}
