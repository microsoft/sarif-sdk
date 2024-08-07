﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Converters.ClangTidyObjectModel
{
    public class ClangTidyDiagnostic
    {
        public string DiagnosticName { get; set; }
        public ClangTidyDiagnosticMessage DiagnosticMessage { get; set; }
        public List<ClangTidyDiagnosticMessage> Notes { get; set; }
    }
}
