// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Flags]
    internal enum TestRuleBehaviors
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
        TreatPlatformAsInvalid = 0x400
    }
}