// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Converters.ClangTidyObjectModel
{
    public class ClangTidyReplacement
    {
        public string FilePath { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public string ReplacementText { get; set; }
    }
}
