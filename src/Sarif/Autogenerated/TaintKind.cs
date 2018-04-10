// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values that classify the code flow location in terms of a taint analysis.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public enum TaintKind
    {
        Unknown,
        Sink,
        Source,
        Sanitizer
    }
}