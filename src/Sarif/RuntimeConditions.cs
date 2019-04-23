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
    public enum RuntimeConditions : int
    {
        None = 0,

        // Not used today but perhaps soon...
        //CouldNotLoadCustomLoggerAssembly,
        //CouldNotLoadCustomLoggerType,
        //UnrecognizedDefaultLoggerExtension,
        //MalformedCustomLoggersArgument,
        //LoggerFailedInitialization,
        //LoggerRaisedExceptionOnInitialization,
        //LoggerRaisedExceptionOnWrite,
        //LoggerRaisedExceptionOnClose,

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
        ExceptionCreatingLogFile = 0x10,
        ExceptionLoadingPdb = 0x20,
        ExceptionInEngine = 0x40,
        ExceptionLoadingTargetFile = 0x80,
        ExceptionLoadingAnalysisPlugin = 0x100,
        NoRulesLoaded = 0x200,
        NoValidAnalysisTargets = 0x400,
        RuleMissingRequiredConfiguration = 0x800,
        TargetParseError = 0x1000,
        MissingFile = 0x2000,
        ExceptionAccessingFile = 0x4000,
        ExceptionInstantiatingSkimmers = 0x8000,

        // Non-fatal conditions
        UnassignedNonfatal        = 0x03F0000,
        RuleWasExplicitlyDisabled = 0x0400000,
        RuleCannotRunOnPlatform   = 0x0800000,
        RuleNotApplicableToTarget = 0x1000000,
        TargetNotValidToAnalyze   = 0x2000000,
        OneOrMoreWarningsFired    = 0x4000000,
        OneOrMoreErrorsFired      = 0x8000000,

        Nonfatal = 0xFFF0000
    }       
}
