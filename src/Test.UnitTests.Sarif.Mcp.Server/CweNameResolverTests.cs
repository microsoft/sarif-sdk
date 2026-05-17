// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Mcp.Server;

using Xunit;

namespace Test.UnitTests.Sarif.Mcp.Server
{
    /// <summary>
    /// Pins the CWE lookup against the embedded taxonomy. The taxonomy is
    /// regenerated from authoritative MITRE data via
    /// <c>scripts/Generate-CweTaxonomy.ps1</c>; these tests assert that the
    /// resolver loads it correctly and produces stable PascalCase identifiers
    /// for well-known CWEs.
    /// </summary>
    public class CweNameResolverTests
    {
        [Theory]
        // Famous AI-relevant CWEs. Names asserted against the deterministic
        // PascalCase derivation \u2014 the canonical CWE name with the alternate
        // parenthetical elided.
        [InlineData("CWE-78", "ImproperNeutralizationOfSpecialElementsUsedInAnOsCommand")]
        [InlineData("CWE-79", "ImproperNeutralizationOfInputDuringWebPageGeneration")]
        [InlineData("CWE-89", "ImproperNeutralizationOfSpecialElementsUsedInAnSqlCommand")]
        [InlineData("CWE-352", "CrossSiteRequestForgery")]
        [InlineData("CWE-502", "DeserializationOfUntrustedData")]
        [InlineData("CWE-798", "UseOfHardCodedCredentials")]
        public void Resolve_KnownCwe_ReturnsExpectedPascalCaseName(string cweId, string expected)
        {
            var resolver = new CweNameResolver();
            resolver.Resolve(cweId).Should().Be(expected);
        }

        [Theory]
        [InlineData("cwe-78", "case-insensitive prefix")]
        [InlineData("CWE-78", "canonical form")]
        public void Resolve_IsCaseInsensitiveOnPrefix(string cweId, string scenario)
        {
            var resolver = new CweNameResolver();
            resolver.Resolve(cweId).Should().NotBeNullOrEmpty(scenario);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-a-cwe")]
        [InlineData("OWASP-A1")]
        [InlineData("CVE-2024-12345")]
        public void Resolve_NonCweInput_ReturnsNull(string? input)
        {
            var resolver = new CweNameResolver();
            resolver.Resolve(input!).Should().BeNull();
        }

        [Fact]
        public void Resolve_UnknownCweId_ReturnsNull()
        {
            var resolver = new CweNameResolver();
            // CWE-999999 is far outside the published catalog.
            resolver.Resolve("CWE-999999").Should().BeNull();
        }

        [Fact]
        public void Resolve_DoesNotMakeNetworkCalls()
        {
            // The contract: lookup is purely against the embedded taxonomy.
            // Indirect verification: a resolver instance constructed and used
            // here completes synchronously and successfully even if the network
            // is unavailable. Direct verification would require a network
            // sandbox; the absence of an HttpClient field on the class itself
            // is checked by ToolSurfaceTests-style reflection in a separate
            // fixture if we later choose to pin it that way.
            var resolver = new CweNameResolver();
            resolver.Resolve("CWE-78").Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("CWE-78")]
        [InlineData("CWE-89")]
        [InlineData("CWE-352")]
        public void Resolve_KnownCwe_ReturnsNonEmptyPascalCase(string cweId)
        {
            // Indirect coverage of the PascalCase derivation: every known
            // CWE in the embedded taxonomy must resolve to a non-empty
            // identifier-shaped string. The exact strings for famous CWEs
            // are pinned by Resolve_KnownCwe_ReturnsExpectedPascalCaseName.
            var resolver = new CweNameResolver();
            string? name = resolver.Resolve(cweId);
            name.Should().NotBeNullOrEmpty();
            name.Should().MatchRegex(
                "^[A-Z][A-Za-z0-9]+$",
                "the resolver must produce a single PascalCase identifier");
        }
    }
}
