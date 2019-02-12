// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A value that specifies the failure level of the result
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public enum FailureLevel
    {
        None,
        Note,
        Warning,
        Error
    }
}