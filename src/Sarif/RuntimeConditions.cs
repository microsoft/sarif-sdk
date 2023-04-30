// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    // This enum is used to identify specific runtime conditions
    // encountered during execution. This mechanism is used by
    // unit tests to ensure that failure conditions travel expected
    // code paths. These conditions are a combination of fatal
    // and non-fatal circumstances
    [Flags]
    public enum RuntimeConditions : long
    {
        None = 0,

        // Fatal conditions that relate to configuration and
        // cause scan to error out before analysis occurs.
        // InvalidCommandLineOption is a useful bit to
        // use as value 0x1. This allows for a consistent
        // return value in cases where a command-line argument
        // for --rich-return-code can't be detected because it
        // fails to parse.
        InvalidCommandLineOption = 1L << 0,
        RuleMissingRequiredConfiguration = 1L << 1,
        OneOrMoreRulesAreIncompatible = 1L << 2,
        NoValidAnalysisTargets = 1L << 3,
        FileAlreadyExists = 1L << 4,
        NoRulesLoaded = 1L << 5,
        MissingFile = 1L << 6,

        FatalReserved0 = 1L << 7,
        FatalReserved1 = 1L << 8,
        FatalReserved2 = 1L << 9,
        FatalReserved3 = 1L << 10,
        FatalReserved4 = 1L << 11,

        // Fatal conditions that occur during analysis.
        ExceptionRaisedInSkimmerCanAnalyze = 1L << 12,
        ExceptionInstantiatingSkimmers = 1L << 13,
        ExceptionLoadingAnalysisPlugin = 1L << 14,
        ExceptionProcessingCommandline = 1L << 15,
        ExceptionInSkimmerInitialize = 1L << 16,
        ExceptionProcessingBaseline = 1L << 17,
        ExceptionCreatingOutputFile = 1L << 18,
        ExceptionLoadingTargetFile = 1L << 19,
        ExceptionInSkimmerAnalyze = 1L << 20,
        ExceptionPostingLogFile = 1L << 21,
        ExceptionAccessingFile = 1L << 22,
        ExceptionLoadingPdb = 1L << 23,
        ExceptionInEngine = 1L << 24,
        TargetParseError = 1L << 25,
        AnalysisCanceled = 1L << 26,
        AnalysisTimedOut = 1L << 27,

        FatalReserved5 = 1L << 28,
        FatalReserved6 = 1L << 29,
        FatalReserved7 = 1L << 30,
        FatalReserved8 = 1L << 31,

        Fatal = uint.MaxValue,
        UnassignedFatal = FatalReserved0 | FatalReserved1 | FatalReserved3 | FatalReserved4 | FatalReserved5 |
                          FatalReserved6 | FatalReserved7 | FatalReserved7 | FatalReserved8,

        // Non-fatal conditions
        OneOrMoreFilesSkippedDueToExceedingSizeLimits = 1L << 32,
        OneOrMoreEmptyFilesSkipped = 1L << 33,
        RuleWasExplicitlyDisabled = 1L << 34,
        RuleNotApplicableToTarget = 1L << 35,
        RuleCannotRunOnPlatform = 1L << 36,
        TargetNotValidToAnalyze = 1L << 37,
        OneOrMoreWarningsFired = 1L << 38,
        OneOrMoreErrorsFired = 1L << 39,
        ObsoleteOption = 1L << 40,

        NonfatalReserved0 = 1L << 41,
        NonfatalReserved1 = 1L << 42,
        NonfatalReserved2 = 1L << 43,
        NonfatalReserved3 = 1L << 44,
        NonfatalReserved4 = 1L << 45,
        NonfatalReserved5 = 1L << 46,
        NonfatalReserved6 = 1L << 47,
        NonfatalReserved7 = 1L << 48,
        NonfatalReserved8 = 1L << 49,
        NonfatalReserved9 = 1L << 50,
        NonfatalReserved10 = 1L << 51,
        NonfatalReserved11 = 1L << 52,
        NonfatalReserved12 = 1L << 53,
        NonfatalReserved13 = 1L << 54,
        NonfatalReserved14 = 1L << 55,
        NonfatalReserved15 = 1L << 56,
        NonfatalReserved16 = 1L << 57,
        NonfatalReserved17 = 1L << 58,
        NonfatalReserved18 = 1L << 59,
        NonfatalReserved19 = 1L << 60,
        NonfatalReserved20 = 1L << 61,
        NonfatalReserved21 = 1L << 62,

        Nonfatal = long.MaxValue ^ uint.MaxValue,
        UnassignedNonfatal =
            NonfatalReserved0 | NonfatalReserved1 | NonfatalReserved2 | NonfatalReserved3 | NonfatalReserved4 |
            NonfatalReserved5 | NonfatalReserved6 | NonfatalReserved7 | NonfatalReserved8 | NonfatalReserved9 |
            NonfatalReserved10 | NonfatalReserved11 | NonfatalReserved12 | NonfatalReserved13 | NonfatalReserved14 |
            NonfatalReserved15 | NonfatalReserved16 | NonfatalReserved17 | NonfatalReserved18 | NonfatalReserved19 |
            NonfatalReserved20 | NonfatalReserved21,
    }
}
