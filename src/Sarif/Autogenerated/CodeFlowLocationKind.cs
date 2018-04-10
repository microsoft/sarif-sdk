// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values specifying the kind of an code flow location.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public enum CodeFlowLocationKind
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