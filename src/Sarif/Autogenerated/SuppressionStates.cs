// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The state of a result relative to a baseline of a previous run.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    [Flags]
    public enum SuppressionStates
    {
        None,
        SuppressedInSource = 0x1,
        SuppressedInBaseline = 0x2
    }
}