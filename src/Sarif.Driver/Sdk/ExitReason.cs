// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    // Be sure to append new values for this enum to avoid changing existing member values.
    public enum ExitReason
    {
        None,

        InvalidCommandLineOption,

        UnhandledExceptionInEngine,
        UnhandledExceptionInstantiatingSkimmers,

        ExceptionPostingLogFile,
        ExceptionCreatingLogFile,
        ExceptionWritingToLogFile,
        ExceptionProcessingBaseline,

        NoRulesLoaded,
        IncompatibleRulesDetected,

        NoValidAnalysisTargets,

        AnalysisCanceled,
        AnalysisTimedOut,
    }
}
