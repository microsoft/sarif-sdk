// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

[assembly: AssemblyVersion(Microsoft.CodeAnalysis.Sarif.VersionConstants.FileVersion)]
[assembly: AssemblyFileVersion(Microsoft.CodeAnalysis.Sarif.VersionConstants.FileVersion)]

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class VersionConstants
    {
        public const string Version = FileVersion + Prerelease;
        public const string FileVersion = "1.4.6";
        public const string Prerelease = "-beta";
    }
}
