// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values specifying the importance of an "annotatedCodeLocation" within the "codeFlow" in which it occurs
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public enum AnnotatedCodeLocationImportance
    {
        Important,
        Essential,
        Unimportant
    }
}