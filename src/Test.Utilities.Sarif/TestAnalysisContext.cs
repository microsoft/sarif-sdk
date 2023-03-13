﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class TestAnalysisContext : AnalyzeContextBase
    {
        public bool Disposed { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            Disposed = true;
        }
    }
}
