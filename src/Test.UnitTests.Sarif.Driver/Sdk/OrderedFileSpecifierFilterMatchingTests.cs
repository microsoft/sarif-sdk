// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Driver
{
    /// <summary>
    /// Tests covering SDK-I: <see cref="OrderedFileSpecifier"/> now post-filters the file
    /// enumerator's results against the documented glob semantics, rejecting the
    /// Win32-short-name false-positives where '*.sarif' would also match files like
    /// 'foo.sarif.to-delete' or 'foo.sarif.bak'.
    /// </summary>
    public class OrderedFileSpecifierFilterMatchingTests
    {
        // Internal type access — these tests live in the same assembly view via InternalsVisibleTo.
        // Use reflection to call the internal helper directly so tests don't have to manufacture
        // an entire filesystem mock for every shape variant.
        private static bool Matches(string fileName, string filter)
        {
            System.Reflection.MethodInfo m = typeof(OrderedFileSpecifier).GetMethod(
                "FilterMatchesFileName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (bool)m.Invoke(null, new object[] { fileName, filter });
        }

        [Theory]
        // The canonical SDK-I bug: '*.sarif' must NOT match files with additional trailing chars.
        [InlineData("foo.sarif", "*.sarif", true)]
        [InlineData("foo.sarif.to-delete", "*.sarif", false)]
        [InlineData("foo.sarif.bak", "*.sarif", false)]
        [InlineData("foo.SARIF", "*.sarif", true)]              // case-insensitive on Windows
        // Prefix glob 'foo*'
        [InlineData("foobar.txt", "foo*", true)]
        [InlineData("xfoobar.txt", "foo*", false)]
        // Contains glob '*foo*'
        [InlineData("xfoobar.txt", "*foo*", true)]
        [InlineData("xbar.txt", "*foo*", false)]
        // Bare '*' and '*.*' admit everything (FindFirstFile parity)
        [InlineData("anything", "*", true)]
        [InlineData("anything.txt", "*.*", true)]
        // Exact match (no wildcards)
        [InlineData("foo.txt", "foo.txt", true)]
        [InlineData("foo.txt.bak", "foo.txt", false)]
        // '?' matches a single char
        [InlineData("a.txt", "?.txt", true)]
        [InlineData("ab.txt", "?.txt", false)]
        public void FilterMatchesFileName_HonorsSuffixGlobSemantics(string fileName, string filter, bool expected)
        {
            Matches(fileName, filter).Should().Be(expected,
                $"file '{fileName}' against filter '{filter}' should match: {expected}");
        }

        [Fact]
        public void Enumeration_RejectsShortName_FalsePositives()
        {
            // End-to-end smoke: create a tempdir with one *.sarif and one *.sarif.to-delete,
            // verify '*.sarif' only enumerates the former. This exercises the actual filesystem
            // matching (incl. any Win32 short-name behavior) rather than just the helper.
            string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                File.WriteAllText(Path.Combine(root, "real.sarif"), "{}");
                File.WriteAllText(Path.Combine(root, "real.sarif.to-delete"), "{}");
                File.WriteAllText(Path.Combine(root, "real.sarif.bak"), "{}");

                var specifier = new OrderedFileSpecifier(Path.Combine(root, "*.sarif"));
                var artifacts = System.Linq.Enumerable.ToList(specifier.Artifacts);

                artifacts.Should().HaveCount(1, "only real.sarif matches the *.sarif glob");
                artifacts[0].Uri.LocalPath.Should().EndWith("real.sarif");
            }
            finally
            {
                if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
            }
        }
    }
}
