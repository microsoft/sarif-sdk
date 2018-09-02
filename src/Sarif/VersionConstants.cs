// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class VersionConstants
    {
        public const string Prerelease = "csd.1.0.2";
        public const string AssemblyVersion = "2.0.0";
        public const string FileVersion = AssemblyVersion + ".0";
        public const string Version = AssemblyVersion + Prerelease;
    }
}