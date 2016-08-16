// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values classifying the annotated code location in context of a taint analysis.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.42.0.0")]
    public enum AnnotatedCodeLocationTaint
    {
        Unknown,
        Sink,
        Source,
        Sanitizer
    }
}