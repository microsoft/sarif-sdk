// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values specifying the kind of an annotated code location.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.42.0.0")]
    public enum AnnotatedCodeLocationKind
    {
        Unknown,
        Alias,
        Assignment,
        Branch,
        Call,
        CallReturn,
        Continuation,
        Declaration,
        FunctionEnter,
        FunctionExit,
        FunctionReturn,
        Usage
    }
}