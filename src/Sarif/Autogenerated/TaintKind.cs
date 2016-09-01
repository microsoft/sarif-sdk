// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values that classify the annotated code location in terms of a taint analysis.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.46.0.0")]
    public enum TaintKind
    {
        Unknown,
        Sink,
        Source,
        Sanitizer
    }
}