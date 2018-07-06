// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// The state of a result relative to a baseline of a previous run.
    /// </summary>
    [Flags]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public enum SuppressionStatesVersionOne
    {
        None,
        SuppressedInSource,
        SuppressedExternally
    }
}