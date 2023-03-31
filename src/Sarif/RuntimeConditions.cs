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
    public enum RuntimeConditions : uint
    {
        None = 0,

        // Fatal conditions
        // InvalidCommandLineOption is a useful bit to
        // use as value 0x1. This allows for a consistent
        // return value in cases where a command-line argument
        // for --rich-return-code can't be detected because it
        // fails to parse.
        InvalidCommandLineOption = 0x1,
        ExceptionInSkimmerInitialize = 0x2,
        ExceptionRaisedInSkimmerCanAnalyze = 0x4,
        ExceptionInSkimmerAnalyze = 0x8,
        ExceptionCreatingOutputFile = 0x10,
        ExceptionLoadingPdb = 0x20,
        ExceptionInEngine = 0x40,
        ExceptionLoadingTargetFile = 0x80,
        ExceptionProcessingCommandline = 0x100,
        ExceptionLoadingAnalysisPlugin = 0x200,
        NoRulesLoaded = 0x400,
        NoValidAnalysisTargets = 0x800,
        RuleMissingRequiredConfiguration = 0x1000,
        TargetParseError = 0x2000,
        MissingFile = 0x4000,
        ExceptionAccessingFile = 0x8000,
        ExceptionInstantiatingSkimmers = 0x10000,
        FileAlreadyExists = 0x20000,
        ExceptionProcessingBaseline = 0x40000,
        ExceptionPostingLogFile = 0x80000,
        OneOrMoreRulesAreIncompatible = 0x100000,
        AnalysisCanceled = 0x200000,
        AnalysisTimedOut = 0x400000,

        // Non-fatal conditions
        UnassignedNonfatal = 0x00C00000,
        OneOrMoreFilesSkippedDueToSize = 0x01000000,
        RuleWasExplicitlyDisabled = 0x02000000,
        RuleCannotRunOnPlatform = 0x04000000,
        RuleNotApplicableToTarget = 0x08000000,
        TargetNotValidToAnalyze = 0x10000000,
        OneOrMoreWarningsFired = 0x20000000,
        OneOrMoreErrorsFired = 0x40000000,
        ObsoleteOption = 0x80000000,

        Nonfatal = 0x7FC00000,
    }
}
