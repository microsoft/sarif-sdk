// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>A bitfield of flags for specifying the method by which XML elements shall be ignored.</summary>
    [Flags]
    internal enum IgnoreOptions
    {
        /// <summary>Requires exactly one of the indicated XML node be present.</summary>
        Required = 0x00,

        /// <summary>Allows no instance of the XML node indicated to be present.</summary>
        Optional = 0x01,

        /// <summary>Ignores an unbounded number of instances of the indicated XML node.</summary>
        Multiple = 0x02
    }
}
