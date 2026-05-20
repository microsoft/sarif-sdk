// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Pins the defensive-read semantics of <see cref="EvidenceJsonReader"/>:
    /// AI evidence entries in the wild emit some properties (notably
    /// <c>backing</c>) as either a single string or as an array of strings;
    /// the reader must accept both shapes without throwing (issue #2908).
    /// </summary>
    public class EvidenceJsonReaderTests
    {
        [Fact]
        public void ReadString_ReturnsString_WhenTokenIsString()
        {
            var entry = JObject.Parse("{ \"strength\": \"demonstrated\" }");
            EvidenceJsonReader.ReadString(entry, "strength").Should().Be("demonstrated");
        }

        [Fact]
        public void ReadString_ReturnsNull_WhenPropertyAbsent()
        {
            var entry = JObject.Parse("{}");
            EvidenceJsonReader.ReadString(entry, "strength").Should().BeNull();
        }

        [Theory]
        [InlineData("{ \"strength\": [\"demonstrated\"] }")]
        [InlineData("{ \"strength\": { \"value\": \"demonstrated\" } }")]
        [InlineData("{ \"strength\": 7 }")]
        [InlineData("{ \"strength\": true }")]
        [InlineData("{ \"strength\": null }")]
        public void ReadString_ReturnsNull_WhenTokenIsNotString(string json)
        {
            var entry = JObject.Parse(json);
            EvidenceJsonReader.ReadString(entry, "strength").Should().BeNull(
                "the defensive reader must never throw on non-string tokens; rule-side logic gates on null");
        }

        [Fact]
        public void ReadStrings_ReturnsSingletonList_WhenTokenIsString()
        {
            var entry = JObject.Parse("{ \"backing\": \"sarif:/runs/0\" }");
            EvidenceJsonReader.ReadStrings(entry, "backing").Should().Equal("sarif:/runs/0");
        }

        [Fact]
        public void ReadStrings_ReturnsArrayElements_WhenTokenIsArrayOfStrings()
        {
            var entry = JObject.Parse(
                "{ \"backing\": [\"sarif:/runs/0\", \"external/path.md\", \"https://example.com\"] }");

            EvidenceJsonReader.ReadStrings(entry, "backing").Should().Equal(
                "sarif:/runs/0", "external/path.md", "https://example.com");
        }

        [Fact]
        public void ReadStrings_SilentlyDropsNonStringElements_FromArray()
        {
            // Mixed-type arrays are non-canonical for ai/evidence backing but
            // should not crash the rule. Non-string elements are dropped.
            var entry = JObject.Parse(
                "{ \"backing\": [\"sarif:/runs/0\", 7, true, null, { \"x\": 1 }, \"external/path.md\"] }");

            EvidenceJsonReader.ReadStrings(entry, "backing").Should().Equal(
                "sarif:/runs/0", "external/path.md");
        }

        [Fact]
        public void ReadStrings_ReturnsEmpty_WhenPropertyAbsent()
        {
            var entry = JObject.Parse("{}");
            EvidenceJsonReader.ReadStrings(entry, "backing").Should().BeEmpty();
        }

        [Fact]
        public void ReadStrings_ReturnsEmpty_WhenEmptyArray()
        {
            var entry = JObject.Parse("{ \"backing\": [] }");
            EvidenceJsonReader.ReadStrings(entry, "backing").Should().BeEmpty();
        }

        [Theory]
        [InlineData("{ \"backing\": { \"value\": \"x\" } }")]
        [InlineData("{ \"backing\": 7 }")]
        [InlineData("{ \"backing\": true }")]
        [InlineData("{ \"backing\": null }")]
        public void ReadStrings_ReturnsEmpty_OnOtherShapes(string json)
        {
            var entry = JObject.Parse(json);
            EvidenceJsonReader.ReadStrings(entry, "backing").Should().BeEmpty();
        }
    }
}
