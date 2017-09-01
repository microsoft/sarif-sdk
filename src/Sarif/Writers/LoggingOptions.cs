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

        // Persist base64 encoded file contents to log.
        PersistFileContents = 0x2,
        
        // Persist environment variables to log file (which may contain security-sensitive information).
        PersistEnvironment = 0x4,

        // Indent persisted JSON for easy file viewing.
        PrettyPrint = 0x8,

        // Persist verbose information to log file, such as informational messages.
        Verbose = 0x10,

        // Overwrite previous version of log file, if it exists.
        OverwriteExistingOutputFile = 0x20,

        All = ComputeFileHashes | PersistEnvironment | PersistFileContents | PrettyPrint | Verbose | OverwriteExistingOutputFile
    }
}
