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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
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
        GeneratedFile = 2048
    }
}