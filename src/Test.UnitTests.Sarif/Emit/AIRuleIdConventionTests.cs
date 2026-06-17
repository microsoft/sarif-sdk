// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text.RegularExpressions;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Emit;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Test.UnitTests.Emit
{
    public class AIRuleIdConventionTests
    {
        // ----- Sub-id form acceptance -----

        [Theory]
        [InlineData("CWE-89/kql-injection-from-config")]
        [InlineData("CWE-79/dom-xss-via-sanitizer-bypass")]
        [InlineData("CWE-1/a")]                                  // minimal viable shape
        [InlineData("CWE-327/md5-usage")]                        // digits allowed inside the sub-id
        [InlineData("CWE-89/2nd-order-injection")]               // leading-digit token is fine
        public void IsAcceptable_ReturnsTrue_ForTaxonomySubIdForm(string ruleId)
        {
            AIRuleIdConvention.IsAcceptable(ruleId).Should().BeTrue();
        }

        // ----- NOVEL- form acceptance -----

        [Theory]
        [InlineData("NOVEL-look-ma-i-hallucinated-outside-of-mitre")]
        [InlineData("NOVEL-prompt-injection-via-system-message")]
        [InlineData("NOVEL-a")]                                  // minimal viable shape
        [InlineData("NOVEL-x509-bypass")]                        // lowercase + digits allowed
        public void IsAcceptable_ReturnsTrue_ForNovelEscapeForm(string ruleId)
        {
            AIRuleIdConvention.IsAcceptable(ruleId).Should().BeTrue();
        }

        // ----- Universal rejections -----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]                                       // whitespace-only
        public void IsAcceptable_ReturnsFalse_ForEmptyOrWhitespace(string ruleId)
        {
            AIRuleIdConvention.IsAcceptable(ruleId).Should().BeFalse();
        }

        // ----- Bare taxonomy rejections -----

        [Theory]
        [InlineData("CWE-89")]                                    // no sub-id
        [InlineData("CWE-89/")]                                   // empty sub-id
        [InlineData("CWE-89/a/b")]                                // sub-id contains slash
        [InlineData("CWE-89/a b")]                                // sub-id contains whitespace
        [InlineData("cwe-89/foo")]                                // lowercase base
        [InlineData("CWE/foo")]                                   // base has no number
        [InlineData("CWE-x/foo")]                                 // base is not numeric
        [InlineData("CWE-89/Foo")]                                // uppercase in sub-id
        [InlineData("CWE-89/a--b")]                               // consecutive hyphens
        [InlineData("CWE-89/a-")]                                 // trailing hyphen
        [InlineData("CWE-89/-a")]                                 // leading hyphen
        [InlineData("CVE-2021-12345/exploit-via-file-upload")]   // CVE no longer accepted (CWE-only)
        [InlineData("OWASP-A01-2021/broken-access-control")]     // OWASP no longer accepted (CWE-only)
        [InlineData("MY-CUSTOM-RULE")]                            // not CWE, not NOVEL-
        public void IsAcceptable_ReturnsFalse_ForMalformedSubIdForm(string ruleId)
        {
            AIRuleIdConvention.IsAcceptable(ruleId).Should().BeFalse();
        }

        // ----- NOVEL- rejections -----

        [Theory]
        [InlineData("NOVEL")]                                     // no dash + sub-id
        [InlineData("NOVEL-")]                                    // empty sub-id
        [InlineData("NOVEL-foo/bar")]                             // NOVEL- form is flat — no slash allowed
        [InlineData("NOVEL-foo-")]                                // trailing dash
        [InlineData("NOVEL--foo")]                                // leading dash after prefix
        [InlineData("NOVEL-a--b")]                                // consecutive hyphens
        [InlineData("NOVEL-Foo")]                                 // uppercase in sub-id
        [InlineData("NOVEL-mixed-Case-123")]                      // mixed case no longer allowed
        [InlineData("novel-foo")]                                 // lowercase prefix
        [InlineData("NOVEL-foo bar")]                             // whitespace in sub-id
        public void IsAcceptable_ReturnsFalse_ForMalformedNovelForm(string ruleId)
        {
            AIRuleIdConvention.IsAcceptable(ruleId).Should().BeFalse();
        }

        // ----- ThrowIfUnacceptable single-id surface -----

        [Fact]
        public void ThrowIfUnacceptable_DoesNothing_OnAcceptableId()
        {
            System.Action act = () => AIRuleIdConvention.ThrowIfUnacceptable("CWE-89/foo");
            act.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfUnacceptable_Throws_OnMalformedId()
        {
            System.Action act = () => AIRuleIdConvention.ThrowIfUnacceptable("CWE-89");

            AIRuleIdConventionException ex = act.Should().Throw<AIRuleIdConventionException>().Which;
            ex.OffendingRuleIds.Should().ContainSingle().Which.Should().Be("CWE-89");
        }

        // ----- ThrowIfAnyUnacceptable batch surface -----

        [Fact]
        public void ThrowIfAnyUnacceptable_DoesNothing_OnEmptyOrNullList()
        {
            System.Action act1 = () => AIRuleIdConvention.ThrowIfAnyUnacceptable(null);
            System.Action act2 = () => AIRuleIdConvention.ThrowIfAnyUnacceptable(new System.Collections.Generic.List<Result>());

            act1.Should().NotThrow();
            act2.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfAnyUnacceptable_DoesNothing_WhenAllResultsConform()
        {
            var results = new System.Collections.Generic.List<Result>
            {
                new() { RuleId = "CWE-79/dom-xss-bypass" },
                new() { RuleId = "NOVEL-prompt-injection" },
                new() { RuleId = "CWE-89/string-concat-query" },
            };

            System.Action act = () => AIRuleIdConvention.ThrowIfAnyUnacceptable(results);
            act.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfAnyUnacceptable_CollectsAllOffenders_InSourceOrder()
        {
            var results = new System.Collections.Generic.List<Result>
            {
                new() { RuleId = "CWE-79" },                       // bare
                new() { RuleId = "CWE-89/ok" },                    // conforms
                new() { RuleId = "my-rule" },                      // not taxonomy, not NOVEL-
                new() { RuleId = "NOVEL-prompt-injection" },       // conforms
                new() { RuleId = "NOVEL-foo/bar" },                // NOVEL- with slash
                new() { RuleId = null },                           // null
            };

            System.Action act = () => AIRuleIdConvention.ThrowIfAnyUnacceptable(results);

            AIRuleIdConventionException ex = act.Should().Throw<AIRuleIdConventionException>().Which;
            ex.OffendingRuleIds.Should().Equal("CWE-79", "my-rule", "NOVEL-foo/bar", string.Empty);
        }

        // ----- Exception message shape (AI-consumable) -----

        [Fact]
        public void Exception_MessageContainsErrorCodeAndBothAcceptedFormsWithExamples()
        {
            var ex = new AIRuleIdConventionException(new[] { "CWE-79", string.Empty });

            ex.Message.Should().Contain(AIRuleIdConventionException.ErrorCode);
            ex.Message.Should().Contain("'CWE-79'");
            ex.Message.Should().Contain("(empty ruleId)");
            ex.Message.Should().Contain("Taxonomy sub-id");
            ex.Message.Should().Contain("CWE-89/kql-injection-from-config");
            ex.Message.Should().Contain("CWE-89/sql-injection");
            ex.Message.Should().Contain("fall back");
            ex.Message.Should().Contain("NOVEL escape hatch");
            ex.Message.Should().Contain("NOVEL-prompt-injection-via-system-message");
            ex.Message.Should().Contain("docs/ai/generating-sarif.md#rule-id-convention");
        }

        [Fact]
        public void Exception_SingleOffender_UsesSingularPhrasing()
        {
            var ex = new AIRuleIdConventionException(new[] { "CWE-79" });
            ex.Message.Should().Contain("1 result did not conform");
            ex.Message.Should().NotContain("1 results");
        }

        [Fact]
        public void Exception_MultipleOffenders_UsesPluralPhrasing()
        {
            var ex = new AIRuleIdConventionException(new[] { "CWE-79", "CWE-89" });
            ex.Message.Should().Contain("2 results did not conform");
        }

        // ----- TryGetCweId: canonical CWE extraction (house style: [Fact] + shared helper) -----

        private static void VerifyTryGetCweId(string ruleId, bool expectedSuccess, string expectedCweId)
        {
            bool success = AIRuleIdConvention.TryGetCweId(ruleId, out string cweId);

            success.Should().Be(expectedSuccess);
            cweId.Should().Be(expectedCweId);
        }

        [Fact]
        public void TryGetCweId_ReturnsCanonicalId_ForBareCweDescriptorId()
        {
            VerifyTryGetCweId("CWE-89", expectedSuccess: true, expectedCweId: "CWE-89");
        }

        [Fact]
        public void TryGetCweId_ReturnsCanonicalId_AndDropsSubId_ForSubIdForm()
        {
            VerifyTryGetCweId("CWE-89/kql-injection", expectedSuccess: true, expectedCweId: "CWE-89");
        }

        [Fact]
        public void TryGetCweId_NormalizesLeadingZeros()
        {
            VerifyTryGetCweId("CWE-089", expectedSuccess: true, expectedCweId: "CWE-89");
            VerifyTryGetCweId("CWE-007/path-traversal", expectedSuccess: true, expectedCweId: "CWE-7");
        }

        [Fact]
        public void TryGetCweId_ReturnsFalse_ForNovelForm()
        {
            VerifyTryGetCweId("NOVEL-prompt-injection", expectedSuccess: false, expectedCweId: null);
        }

        [Fact]
        public void TryGetCweId_ReturnsFalse_ForNullEmptyOrWhitespace()
        {
            VerifyTryGetCweId(null, expectedSuccess: false, expectedCweId: null);
            VerifyTryGetCweId(string.Empty, expectedSuccess: false, expectedCweId: null);
            VerifyTryGetCweId("   ", expectedSuccess: false, expectedCweId: null);
        }

        [Fact]
        public void TryGetCweId_ReturnsFalse_ForNonCweRuleIds()
        {
            VerifyTryGetCweId("cwe-89/foo", expectedSuccess: false, expectedCweId: null);   // lowercase base
            VerifyTryGetCweId("CWE-x/foo", expectedSuccess: false, expectedCweId: null);    // non-numeric
            VerifyTryGetCweId("MY-CUSTOM-RULE", expectedSuccess: false, expectedCweId: null);
        }

        // ----- Cited-doc anchor must not rot -----
        // Every runtime ruleId-convention error directs the reader to
        // docs/ai/generating-sarif.md#rule-id-convention. That fragment is only
        // useful if the heading it targets still exists, so gate both ends here:
        // the heading must be present, and the anchor the exception cites must be
        // the GitHub slug of that heading. Rename one without the other and this
        // fails.
        private const string GeneratingSarifRelativePath = "docs/ai/generating-sarif.md";
        private const string RuleIdConventionHeading = "## Rule-ID Convention";

        [Fact]
        public void ExceptionMessage_CitesLiveAnchorInGeneratingSarifDoc()
        {
            string docPath = FindRepoFile(GeneratingSarifRelativePath);
            string[] lines = File.ReadAllLines(docPath);

            string heading = Array.Find(lines, line => line.Trim() == RuleIdConventionHeading);
            heading.Should().NotBeNull(
                $"{GeneratingSarifRelativePath} must contain a '{RuleIdConventionHeading}' heading — " +
                "runtime errors cite its anchor for the full grammar.");

            string anchor = GitHubAnchor(RuleIdConventionHeading.TrimStart('#', ' '));
            string expectedCitation = GeneratingSarifRelativePath + "#" + anchor;

            var ex = new AIRuleIdConventionException(new[] { "CWE-79" });
            ex.Message.Should().Contain(expectedCitation,
                "the exception must point at the live heading anchor; if the heading is renamed, " +
                "update the citation in AIRuleIdConventionException, AddResultsCommand, and ReportingDescriptorEmitter.");
        }

        // GitHub heading-slug algorithm: lowercase, drop characters that are not
        // word/space/hyphen, then replace spaces with hyphens.
        private static string GitHubAnchor(string heading)
        {
            string lowered = heading.ToLowerInvariant();
            string stripped = Regex.Replace(lowered, @"[^\w\- ]", string.Empty);
            return stripped.Replace(' ', '-');
        }

        private static string FindRepoFile(string relativePath)
        {
            for (DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
                 dir != null;
                 dir = dir.Parent)
            {
                string candidate = Path.Combine(dir.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new FileNotFoundException(
                $"Could not locate {relativePath} by walking up from {AppContext.BaseDirectory}");
        }
    }
}
