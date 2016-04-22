// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values specifying the category of the result.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.14.0.0")]
    public enum ResultKind
    {
        Unknown,
        Error,
        Warning,
        Pass,
        Note,
        NotApplicable,
        InternalError,
        ConfigurationError
    }
}