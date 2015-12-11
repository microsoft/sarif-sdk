// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

[assembly: AssemblyVersion(Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.VersionConstants.FileVersion)]
[assembly: AssemblyFileVersion(Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.VersionConstants.FileVersion)]

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat
{
    public static class VersionConstants
    {
        public const string Version = FileVersion + Prerelease;
        public const string FileVersion = "1.4.5";
        public const string Prerelease = "-beta";
    }
}
