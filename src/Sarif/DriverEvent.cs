// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.CodeAnalysis.Sarif
{
    public enum DriverEvent : int
    {
        EnumerateArtifactsStart = 1,
        EnumerateArtifactsStop = 2,
        FirstArtifactQueued = 3,
        ArtifactSizeInBytes = 4,
        ReadArtifactStart = 5,
        ReadArtifactStop = 6,
        ScanArtifactStart = 7,
        ScanArtifactStop = 8,
        RuleStart = 9,
        RuleStop = 10,
        RuleFired = 11,
        LogResultsStart = 12,
        LogResultsStop = 13,
        RuleReserved0 = 14,
        RuleReserved1Start = 15,
        RuleReserved1Stop = 16,
        ArtifactReserved0 = 17,
        ArtifactReserved1Start = 18,
        ArtifactReserved1Stop = 19,
    }
}
