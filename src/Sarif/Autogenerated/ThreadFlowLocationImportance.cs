// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values specifying the importance of an "threadFlowLocation" within the "codeFlow" in which it occurs
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public enum ThreadFlowLocationImportance
    {
        Important,
        Essential,
        Unimportant
    }
}