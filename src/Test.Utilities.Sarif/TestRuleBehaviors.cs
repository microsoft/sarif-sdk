// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Flags]
    public enum TestRuleBehaviors
    {
        None = 0,

        // Produce an error for every scan target.
        LogError = 0x1,

        // Various code paths where we want to ensure behavior if an exception is raised.
        RaiseExceptionAccessingId = 0x2,
        RaiseExceptionAccessingName = 0x4,
        RaiseExceptionInvokingConstructor = 0x8,
        RaiseExceptionInvokingAnalyze = 0x10,
        RaiseExceptionInvokingCanAnalyze = 0x20,
        RaiseExceptionInvokingInitialize = 0x40,
        RaiseExceptionValidatingOptions = 0x80,

        // Force introduction of errors on parsing a target or failure to locate a PDB.
        RaiseTargetParseError = 0x100,

        // Force 'current platform not valid to analyze' code path.
        TreatPlatformAsInvalid = 0x400,

        // Return 'not applicable' for all analysis targets in CanAnalyze
        RegardAnalysisTargetAsNotApplicable = 0x800,

        // Treat analysis target as a corrupted file
        RegardAnalysisTargetAsCorrupted = 0x1000,

        // Allow analysis for all targets
        RegardAnalysisTargetAsInvalid = 0x2000,

        // Assume one or more options are invalid
        RegardOptionsAsInvalid = 0x4000,

        RaiseExceptionProcessingBaseline = 0x8000,
        RaiseExceptionPostingLogFile = 0x10000,
        RaiseStackOverflowException = 0x20000
    }
}
