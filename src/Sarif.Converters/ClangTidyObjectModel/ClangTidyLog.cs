// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Converters.ClangTidyObjectModel
{
    public class ClangTidyLog
    {
        public string MainSourceFile { get; set; }
        public List<ClangTidyDiagnostic> Diagnostics { get; set; }
    }
}
