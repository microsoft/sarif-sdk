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
    public enum RuntimeConditions
    {
        NoErrors = 0,

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
        ExceptionInstantiatingSkimmers = 0x01,
        ExceptionInSkimmerInitialize = 0x02,
        ExceptionRaisedInSkimmerCanAnalyze = 0x04,
        ExceptionInSkimmerAnalyze = 0x08,
        ExceptionCreatingLogfile = 0x10,
        ExceptionLoadingPdb = 0x20,
        ExceptionInEngine = 0x40,
        ExceptionLoadingTargetFile = 0x80,
        ExceptionLoadingAnalysisPlugIn = 0x100,
        NoRulesLoaded = 0x200,
        NoValidAnalysisTargets = 0x400,
        RuleMissingRequiredConfiguration = 0x800,
        TargetParseError = 0x1000,
        MissingFile = 0x2000,
        ExceptionAccessingFile = 0x4000,
        InvalidCommandLineOption = 0x8000,

        Fatal = (Int32.MaxValue ^ NonFatal),

        // Non-fatal conditions
        RuleNotApplicableToTarget = 0x10000000,
        TargetNotValidToAnalyze   = 0x20000000,

        NonFatal = 0x70000000
    }       
}
