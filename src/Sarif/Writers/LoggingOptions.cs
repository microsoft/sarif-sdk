// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    [Flags]
    public enum LoggingOptions
    {
        None = 0,

        // Indent persisted JSON for easy file viewing.
        PrettyPrint = 0x1,

        // Suppress all non-errors to the console and results file
        Quiet = 0x2,

        // Overwrite previous version of log file, if it exists.
        OverwriteExistingOutputFile = 0x4,

        // Omit redundant properties, producing a smaller but non-human-readable log.
        Optimize = 0x8,

        All = PrettyPrint | Quiet | OverwriteExistingOutputFile | Optimize
    }
}
