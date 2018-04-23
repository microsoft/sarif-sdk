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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public enum FileRole
    {
        Default,
        AnalysisTarget,
        Attachment,
        ResponseFile,
        ResultFile,
        Screenshot,
        StandardStream,
        TraceFile
    }
}