// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public enum ExceptionCondition
    {
        None,
        AccessingId,
        AccessingName,
        InvokingConstructor,
        InvokingAnalyze,
        InvokingCanAnalyze,
        InvokingInitialize
    }
}
