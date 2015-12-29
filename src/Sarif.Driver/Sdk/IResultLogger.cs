// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public interface IResultLogger
    {
        // Once further Result construction utilities are available, this is a sensible overload
        //void Log(Result result);

        void Log(ResultKind messageKind, IAnalysisContext context, Region region, string formatSpecifierId, params string[] arguments);
    }
}
