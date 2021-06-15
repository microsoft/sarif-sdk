// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Converters.ClangTidyObjectModel
{
    public class ClangTidyDiagnosticMessage
    {
        public string Message { get; set; }
        public string FilePath { get; set; }
        public int FileOffset { get; set; }
        public List<ClangTidyReplacement> Replacements { get; set; }
    }
}
