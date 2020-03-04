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
        OutputFileAlreadyExists = 0x10000,

        // Non-fatal conditions
        UnassignedNonfatal = 0x01F00000,
        RuleWasExplicitlyDisabled = 0x02000000,
        RuleCannotRunOnPlatform = 0x04000000,
        RuleNotApplicableToTarget = 0x08000000,
        TargetNotValidToAnalyze = 0x10000000,
        OneOrMoreWarningsFired = 0x20000000,
        OneOrMoreErrorsFired = 0x40000000,

        Nonfatal = 0x7FF00000
    }
}
