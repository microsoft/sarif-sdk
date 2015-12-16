// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Microsoft.CodeAnalysis.Sarif.Sdk
{
    public interface IResultLogger<TContext> where TContext : IAnalysisContext
    {
        void Log(ResultKind messageKind, TContext context, string message);

        void Log(ResultKind messageKind, TContext context, FormattedMessage message);
    }
}
