// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public enum DriverEventId : int
    {
        EnumerateArtifactsStart = 1,
        EnumerateArtifactsStop = 2,
        FirstArtifactQueued = 3,
        ReadArtifactStart = 4,
        ReadArtifactStop = 5,
        ArtifactNotScanned = 6,
        ScanArtifactStart = 7,
        ScanArtifactStop = 8,
        RuleNotCalled = 9,
        RuleStart = 10,
        RuleStop = 11,
        RuleFired = 12,
        LogResultsStart = 13,
        LogResultsStop = 14,
        RuleReserved0 = 15,
        RuleReserved1Start = 16,
        RuleReserved1Stop = 17,
        ArtifactReserved0 = 18,
        ArtifactReserved1Start = 19,
        ArtifactReserved1Stop = 20,
        SessionEnded = 21,
    }
}
