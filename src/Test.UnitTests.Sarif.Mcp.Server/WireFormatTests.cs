// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Newtonsoft.Json.Linq;

using Test.UnitTests.Sarif.Mcp.Server.Fixtures;

using Xunit;

namespace Test.UnitTests.Sarif.Mcp.Server
{
    /// <summary>
    /// Pins wire-format invariants that typed assertions cannot catch:
    /// enums must serialize as lowercase strings (not integers), version
    /// must be the literal string "2.1.0", GUIDs must round-trip as
    /// hyphenated 36-character strings.
    /// </summary>
    public sealed class WireFormatTests : McpScratchTestBase
    {
        [Fact]
        public void Wire_Version_IsLiteralString_2_1_0()
        {
            string outputPath = ScratchPath("wire-version.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            RawJson raw = RawJson.OfFile(outputPath);
            raw.StringAt("$.version").Should().Be("2.1.0");
        }

        [Fact]
        public void Wire_ColumnKind_IsLowercaseString_NotInteger()
        {
            string outputPath = ScratchPath("wire-columnkind.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            RawJson raw = RawJson.OfFile(outputPath);
            raw.TypeAt("$.runs[0].columnKind").Should().Be(
                JTokenType.String,
                "ColumnKind enums must serialize as lowercase strings per SARIF \u00a73.14.5");
            raw.StringAt("$.runs[0].columnKind").Should().Be("utf16CodeUnits");
        }

        [Fact]
        public void Wire_RunGuid_IsHyphenated_LowercaseHex_36Chars()
        {
            string outputPath = ScratchPath("wire-guid.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            RawJson raw = RawJson.OfFile(outputPath);
            string runGuid = raw.StringAt("$.runs[0].automationDetails.guid");

            runGuid.Should().MatchRegex(
                "^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
                "SARIF \u00a73.5.3: GUID values are emitted as the canonical hyphenated 36-character form");
        }

        [Fact]
        public void Wire_ResultLevel_IsLowercaseString_NotInteger()
        {
            string outputPath = ScratchPath("wire-level.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            RawJson raw = RawJson.OfFile(outputPath);
            raw.TypeAt("$.runs[0].results[0].level").Should().Be(
                JTokenType.String,
                "FailureLevel enums must serialize as lowercase strings per SARIF \u00a73.27.10");
            raw.StringAt("$.runs[0].results[0].level").Should().Be("error");
        }

        [Fact]
        public void Wire_ArtifactHash_IsUppercaseHex_64Chars()
        {
            string outputPath = ScratchPath("wire-hash.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            RawJson raw = RawJson.OfFile(outputPath);
            string hash = raw.StringAt("$.runs[0].artifacts[0].hashes['sha-256']");
            hash.Should().MatchRegex("^[0-9A-F]{64}$",
                "SDK HashUtilities emits uppercase hex (named wire-output delta from the AI plug-in's lowercase \u2014 see PR-A). " +
                "SARIF \u00a73.6.2 does not mandate case; downstream consumers should be case-insensitive.");
        }
    }
}
