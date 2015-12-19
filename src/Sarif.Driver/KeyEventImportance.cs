// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Kinds of importance that can be applied to a <see cref="PREfastKeyEvent"/>.</summary>
    public enum KeyEventImportance
    {
        /// <summary>This <see cref="PREfastKeyEvent"/> (and the SFA to which it is attached) is
        /// essential to understand the defect.</summary>
        Essential,

        /// <summary>This <see cref="PREfastKeyEvent"/> describes the defect, but may not be
        /// absolutely necessary to understand its impact.</summary>
        Full
    }
}
