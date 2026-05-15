// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    /// <summary>
    /// Tests covering SDK-J: <see cref="GitHelper"/> now normalizes the directory separators in
    /// any path it surfaces to callers. <c>git</c> always emits POSIX-style forward slashes
    /// regardless of host OS; callers on Windows expect platform-native separators so that
    /// <c>Path.Combine</c>, <see cref="IFileSystem.FileExists"/>, and dictionary key
    /// comparisons against earlier <see cref="System.IO.DirectoryInfo.FullName"/> outputs all
    /// round-trip cleanly.
    /// </summary>
    public class GitHelperPathNormalizationTests
    {
        private static string CallNormalize(string path)
        {
            // The helper is internal; reflect into it to keep tests independent of any future
            // public surface decisions.
            MethodInfo m = typeof(GitHelper).GetMethod(
                "NormalizeDirectorySeparators",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (string)m.Invoke(null, new object[] { path });
        }

        [Fact]
        public void Normalize_ReplacesForwardSlashes_OnWindows()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { return; }

            CallNormalize("C:/src/sarif-sdk").Should().Be(@"C:\src\sarif-sdk");
            CallNormalize("/repo/sub/dir").Should().Be(@"\repo\sub\dir");
        }

        [Fact]
        public void Normalize_ReplacesBackslashes_OnPosix()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { return; }

            CallNormalize(@"home\user\repo").Should().Be("home/user/repo");
        }

        [Fact]
        public void Normalize_IsIdentity_ForAlreadyNativePath()
        {
            // Whichever separator is native on this host, an already-native path should be unchanged.
            string nativePath = "a" + Path.DirectorySeparatorChar + "b" + Path.DirectorySeparatorChar + "c";
            CallNormalize(nativePath).Should().Be(nativePath);
        }

        [Fact]
        public void Normalize_HandlesNullAndEmpty()
        {
            CallNormalize(null).Should().BeNull();
            CallNormalize(string.Empty).Should().Be(string.Empty);
        }

        [Fact]
        public void Normalize_HandlesMixedSeparators_OnWindows()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { return; }

            CallNormalize(@"C:\src/sarif-sdk/sub\dir").Should().Be(@"C:\src\sarif-sdk\sub\dir");
        }
    }
}
