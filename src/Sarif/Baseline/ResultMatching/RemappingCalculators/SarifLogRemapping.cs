// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    public class SarifLogRemapping
    {
        public Func<ExtractedResult, bool> Applies { get; private set; }

        public Func<ExtractedResult, ExtractedResult> RemapResult { get; private set; }
    }
}