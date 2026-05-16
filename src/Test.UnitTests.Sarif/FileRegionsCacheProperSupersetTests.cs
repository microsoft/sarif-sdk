// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    /// <summary>
    /// Tests covering SDK-D: <see cref="FileRegionsCache.ConstructMultilineContextSnippet"/> now
    /// returns <c>null</c> (rather than a clone of the input region) when no proper-superset
    /// context can be synthesized. Historical behavior emitted contextRegion == region, which is
    /// explicitly forbidden by SARIF §3.30 / SARIF1008.
    /// </summary>
    public class FileRegionsCacheProperSupersetTests
    {
        [Fact]
        public void ConstructMultilineContextSnippet_ReturnsNull_WhenRegionExceedsBigSnippetLength()
        {
            // BIGSNIPPETLENGTH = 512. A region at or above the threshold cannot be expanded
            // within the size budget while remaining a PROPER superset.
            string fileContent = $"{new string('a', 200)}{new string('b', 800)}";
            var uri = new Uri(@"c:\temp\proper-superset-test.cpp");
            var region = new Region
            {
                CharOffset = 50,
                CharLength = 600,
            };

            var cache = new FileRegionsCache();
            region = cache.PopulateTextRegionProperties(region, uri, populateSnippet: true, fileText: fileContent);

            Region contextRegion = cache.ConstructMultilineContextSnippet(region, uri, fileContent);

            contextRegion.Should().BeNull("contextRegion cannot be a proper superset when region is already >= BIGSNIPPETLENGTH");
        }

        [Fact]
        public void ConstructMultilineContextSnippet_ReturnsNull_WhenRegionIsTheEntireFile()
        {
            // No room to expand at all — return null rather than emit a clone.
            string fileContent = "baz";
            var uri = new Uri(@"c:\temp\full-file-region.cpp");
            var region = new Region { CharOffset = 0, CharLength = fileContent.Length };

            var cache = new FileRegionsCache();
            region = cache.PopulateTextRegionProperties(region, uri, populateSnippet: true, fileText: fileContent);

            Region contextRegion = cache.ConstructMultilineContextSnippet(region, uri, fileContent);

            contextRegion.Should().BeNull("region spans the whole file; no surrounding context exists");
        }

        [Fact]
        public void ConstructMultilineContextSnippet_ReturnsProperSuperset_OnTypicalCase()
        {
            // Region "baz" inside a 9-char one-line file (with newline padding to allow line-expand path).
            string fileContent = "x" + Environment.NewLine + "baz" + Environment.NewLine + "y";
            int index = fileContent.IndexOf("baz", StringComparison.Ordinal);
            var uri = new Uri(@"c:\temp\proper-superset-typical.cpp");
            var region = new Region { CharOffset = index, CharLength = 3 };

            var cache = new FileRegionsCache();
            region = cache.PopulateTextRegionProperties(region, uri, populateSnippet: true, fileText: fileContent);

            Region contextRegion = cache.ConstructMultilineContextSnippet(region, uri, fileContent);

            contextRegion.Should().NotBeNull();
            contextRegion.CharLength.Should().BeGreaterThan(region.CharLength,
                "contextRegion must be a PROPER superset per SARIF §3.30 (strictly larger than region)");
            contextRegion.Snippet.Text.Should().Contain("baz");
        }

        [Fact]
        public void ConstructMultilineContextSnippet_ReturnsNull_WhenLargeSingleLineFile_RegionAlreadyAtLimit()
        {
            // The char-offset fallback path: ensure we also return null (instead of an equal-size
            // region or an assertion violation) when even expansion-by-SMALLSNIPPETLENGTH fails to
            // grow the region. This is the case the old `Debug.Assert(originalRegion.CharLength
            // <= multilineContextSnippet.CharLength)` was masking — tighten to strict-less-than.
            string fileContent = new string('a', 600);
            var uri = new Uri(@"c:\temp\proper-superset-single-line.cpp");
            var region = new Region
            {
                CharOffset = 0,
                CharLength = 600, // already >= BIGSNIPPETLENGTH, early-return path
            };

            var cache = new FileRegionsCache();
            region = cache.PopulateTextRegionProperties(region, uri, populateSnippet: true, fileText: fileContent);

            Region contextRegion = cache.ConstructMultilineContextSnippet(region, uri, fileContent);

            contextRegion.Should().BeNull();
        }
    }
}
