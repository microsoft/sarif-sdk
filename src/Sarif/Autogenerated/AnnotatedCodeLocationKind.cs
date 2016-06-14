// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A descriptive identifier that categorizes the annotation.
    /// </summary>
    [Flags]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.42.0.0")]
    public enum AnnotatedCodeLocationKind
    {
        None,
        Alias,
        Assignment,
        Branch,
        Call,
        CallReturn,
        Continuation,
        Declaration,
        FunctionEnter,
        FunctionExit,
        Sanitizer,
        Sink,
        Source,
        Usage
    }
}