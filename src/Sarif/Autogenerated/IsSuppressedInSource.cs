// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Indicates whether a result has been suppressed in source (or if that information is unknown).
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.16.0.0")]
    public enum IsSuppressedInSource
    {
        Unknown,
        Suppressed,
        NotSuppressed
    }
}