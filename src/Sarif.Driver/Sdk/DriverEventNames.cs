// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public sealed class DriverEventNames
    {
        public const string ArtifactNotScanned = nameof(ArtifactNotScanned);

        public const string EnumerateArtifacts = nameof(EnumerateArtifacts);
        public const string EnumerateArtifactsStop = $"{EnumerateArtifacts}/Stop";
        public const string EnumerateArtifactsStart = $"{EnumerateArtifacts}/Start";

        public const string ReadArtifact = nameof(ReadArtifact);
        public const string ReadArtifactStop = $"{ReadArtifact}/Stop";
        public const string ReadArtifactStart = $"{ReadArtifact}/Start";

        public const string Scanned = nameof(Scanned);

        public const string ScanArtifact = nameof(ScanArtifact);
        public const string ScanArtifactStop = $"{ScanArtifact}/Stop";
        public const string ScanArtifactStart = $"{ScanArtifact}/Start";

        // Reasons that an artifact might be skipped entirely.
        public const string EmptyFile = nameof(EmptyFile);
        public const string FilePathDenied = nameof(FilePathDenied);
        public const string FilePathNotAllowed = nameof(FilePathNotAllowed);
        public const string FileExceedsSizeLimits = nameof(FileExceedsSizeLimits);

        public const string RuleNotCalled = nameof(RuleNotCalled);
    }
}
