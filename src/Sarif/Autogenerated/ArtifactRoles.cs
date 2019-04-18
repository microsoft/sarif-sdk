// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values specifying the roles played by the file in the analysis.
    /// </summary>
    [Flags]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public enum ArtifactRoles
    {
        None,
        AnalysisTarget = 1,
        Attachment = 2,
        ResponseFile = 4,
        ResultFile = 8,
        StandardStream = 16,
        TraceFile = 32,
        UnmodifiedFile = 64,
        ModifiedFile = 128,
        AddedFile = 256,
        DeletedFile = 512,
        RenamedFile = 1024,
        UncontrolledFile = 2048,
        Driver = 4096,
        Extension = 8192,
        Translation = 16384,
        Taxonomy = 32768,
        Policy = 65536,
        ReferencedOnCommandLine = 131072,
        MemoryContents = 262144,
        Directory = 524288,
        UserSpecifiedConfiguration = 1048576,
        ToolSpecifiedConfiguration = 2097152
    }
}