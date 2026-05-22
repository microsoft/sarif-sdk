// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Taxonomies
{
    public class CweTaxonomyTests
    {
        [Fact]
        public void Load_DefaultStatuses_ReturnsStableDraftAndIncompleteEntries()
        {
            SarifLog log = CweTaxonomy.Load();

            log.Should().NotBeNull();
            log.Runs.Should().ContainSingle();
            log.Runs[0].Taxonomies.Should().ContainSingle();

            var statuses = log.Runs[0].Taxonomies[0].Taxa
                .Select(t => t.TryGetProperty("cwe/status", out string s) ? s : null)
                .Distinct()
                .OrderBy(s => s)
                .ToArray();

            statuses.Should().BeEquivalentTo(new[] { "Draft", "Incomplete", "Stable" });
        }

        [Fact]
        public void Load_All_Returns969Entries()
        {
            SarifLog log = CweTaxonomy.Load(CweStatus.All);
            log.Runs[0].Taxonomies[0].Taxa.Count.Should().Be(969);
        }

        [Theory]
        [InlineData(CweStatus.Stable, 26)]
        [InlineData(CweStatus.Draft, 432)]
        [InlineData(CweStatus.Incomplete, 486)]
        [InlineData(CweStatus.Deprecated, 25)]
        [InlineData(CweStatus.Obsolete, 0)]
        public void Load_SingleStatus_ReturnsExpectedEntryCount(CweStatus status, int expected)
        {
            SarifLog log = CweTaxonomy.Load(status);
            log.Runs[0].Taxonomies[0].Taxa.Count.Should().Be(expected);
        }

        [Fact]
        public void Load_ContainsCweSeventyNineWithExpectedShape()
        {
            SarifLog log = CweTaxonomy.Load(CweStatus.Stable);

            ReportingDescriptor xss = log.Runs[0].Taxonomies[0].Taxa
                .Single(t => t.Id == "CWE-79");

            xss.Name.Should().Contain("Cross-site Scripting");
            xss.ShortDescription.Text.Should().NotBeNullOrWhiteSpace();
            xss.HelpUri.OriginalString.Should().Be("https://cwe.mitre.org/data/definitions/79.html");
            xss.Help.Markdown.Should().Contain("## Description");

            xss.TryGetProperty("cwe/status", out string status).Should().BeTrue();
            status.Should().Be("Stable");
            xss.TryGetProperty("cwe/abstraction", out string abstr).Should().BeTrue();
            abstr.Should().Be("Base");
            xss.TryGetProperty("cwe/parent", out string parent).Should().BeTrue();
            parent.Should().Be("CWE-74");
        }

        [Fact]
        public void Load_DefaultStatusesIncludesSsrf_WhichIsIncompleteUpstream()
        {
            // The whole reason DefaultStatuses includes Incomplete: SSRF (CWE-918) is an OWASP
            // Top 10 entry that MITRE still marks Incomplete. A Stable|Draft-only default would
            // silently exclude it.
            SarifLog log = CweTaxonomy.Load();
            ReportingDescriptor ssrf = log.Runs[0].Taxonomies[0].Taxa.SingleOrDefault(t => t.Id == "CWE-918");
            ssrf.Should().NotBeNull();
            ssrf.TryGetProperty("cwe/status", out string status).Should().BeTrue();
            status.Should().Be("Incomplete");
        }

        [Fact]
        public void Load_DefaultStatusesExcludesDeprecated()
        {
            SarifLog log = CweTaxonomy.Load();
            log.Runs[0].Taxonomies[0].Taxa
                .Where(t => t.TryGetProperty("cwe/status", out string s) && s == "Deprecated")
                .Should().BeEmpty();
        }

        [Fact]
        public void Load_None_Throws()
        {
            Action act = () => CweTaxonomy.Load(CweStatus.None);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Load_All_ReturnsCanonicalLogByReferenceNotCopy()
        {
            SarifLog first = CweTaxonomy.Load(CweStatus.All);
            SarifLog second = CweTaxonomy.Load(CweStatus.All);
            second.Should().BeSameAs(first);
        }

        [Fact]
        public void LoadBrief_All_ReturnsVerbatimEmbeddedCanonicalText()
        {
            string brief = CweTaxonomy.LoadBrief(CweStatus.All);

            brief.Should().StartWith("# CWE");
            brief.Should().Contain("| ID | Name | Abstraction | Status | Parent | Description |");
            brief.Should().Contain("| CWE-79 |");
            brief.Should().Contain("| CWE-918 |");
        }

        [Fact]
        public void LoadBrief_DefaultStatuses_ReRendersFilteredTable()
        {
            string brief = CweTaxonomy.LoadBrief();

            brief.Should().Contain("Compact one-row-per-entry");
            brief.Should().Contain("| ID | Name | Abstraction | Status | Parent | Description |");
            brief.Should().Contain("| CWE-79 ");
            brief.Should().Contain("| CWE-918 ");

            // Deprecated entries (e.g. CWE-2) must not appear in default loadout.
            brief.Should().NotContain("| CWE-2 |");
        }

        [Fact]
        public void LoadBrief_StableOnly_HasTwentySixDataRows()
        {
            string brief = CweTaxonomy.LoadBrief(CweStatus.Stable);

            int dataRows = brief
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Count(line => line.StartsWith("| CWE-", StringComparison.Ordinal));
            dataRows.Should().Be(26);
        }

        [Fact]
        public void LoadBrief_None_Throws()
        {
            Action act = () => CweTaxonomy.LoadBrief(CweStatus.None);
            act.Should().Throw<ArgumentException>();
        }
    }
}
