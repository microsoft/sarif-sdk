// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

[assembly: AssemblyVersion(Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.VersionConstants.AssemblyVersion)]
[assembly: AssemblyFileVersion(Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.VersionConstants.FileVersion)]

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat
{
    public static class VersionConstants
    {
        public const string Version = "1.4.1-beta";
        public const string AssemblyVersion = "1.4.1";
        public const string FileVersion = AssemblyVersion;
    }
}
