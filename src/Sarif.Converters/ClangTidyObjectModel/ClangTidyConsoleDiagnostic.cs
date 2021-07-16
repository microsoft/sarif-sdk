// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Converters.ClangTidyObjectModel
{
    public class ClangTidyConsoleDiagnostic
    {
        public string DiagnosticName { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
        public int ColumnNumber { get; set; }
        public int LineNumber { get; set; }
    }
}
