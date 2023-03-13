#if DEBUG
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
#pragma warning disable CS0618
    public class AnalyzeTestContext : AnalyzeContextBase
#pragma warning restore CS0618
    {
        public override void Dispose()
        {
        }
    }
}
#endif
