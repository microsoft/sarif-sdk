// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    [Flags]
    public enum LoggingOptions
    {
        None = 0,

        // Persist SHA256 hash of all referenced files to log file.
        ComputeFileHashes = 0x1,

        // Persist base64 encoded non-binary file contents to log.
        PersistTextFileContents = 0x2,

        // Persist base64 encoded binary file contents to log.
        PersistBinaryContents = 0x4,

        // Persist environment variables to log file (which may contain security-sensitive information).
        PersistEnvironment = 0x8,

        // Indent persisted JSON for easy file viewing.
        PrettyPrint = 0x10,

        // Persist verbose information to log file, such as informational messages.
        Verbose = 0x20,

        // Overwrite previous version of log file, if it exists.
        OverwriteExistingOutputFile = 0x40,

        All = ComputeFileHashes | PersistEnvironment | PersistTextFileContents | PersistBinaryContents | PrettyPrint | Verbose | OverwriteExistingOutputFile
    }
}
