// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Provides the status of a result relative to a baseline.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.16.0.0")]
    public enum BaselineStatus
    {
        None,
        New,
        Existing,
        Absent
    }
}