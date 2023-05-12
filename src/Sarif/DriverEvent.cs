// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.CodeAnalysis.Sarif
{
    public enum DriverEvent : int
    {
        EnumerateTargetsStart = 1,
        EnumerateTargetsStop = 2,
        ArtifactSizeInBytes = 3,
        GetTargetStart = 4,
        GetTargetStop = 5,
        ScanTargetStart = 6,
        ScanTargetStop = 7,
        RuleStart = 8,
        RuleStop = 9,
        RuleFired = 10,
        LogResultsStart = 11,
        LogResultsEnd = 12,
        RuleReserved = 13,
        RuleReservedStart = 14,
        RuleReservedStop = 15,
        TargetReserved = 16,
        TargetReservedStart = 17,
        TargetReservedStop = 18,
    }
}
