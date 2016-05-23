// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The state of a result relative to a baseline of a previous run.
    /// </summary>
    [Flags]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.35.0.0")]
    public enum SuppressionStates
    {
        None,
        SuppressedInSource = 1
    }
}