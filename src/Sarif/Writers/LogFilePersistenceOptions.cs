// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Flags]
    public enum FilePersistenceOptions
    {
        None = 0,

        // Indent persisted JSON for easy file viewing.
        PrettyPrint = 0x1,

        // Minimize persisted JSON for compactness.
        Minify = 0x2,

        // Overwrite previous version of log file, if it exists.
        ForceOverwrite = 0x4,

        // Inline outputs to files where appropriate.
        Inline = 0x8,

        // Omit redundant properties, producing a smaller but non-human-readable log.
        Optimize = 0x10,
    }
}
