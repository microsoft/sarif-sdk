// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Taxonomies
{
    public class CweTaxonomyEnricherTests
    {
        [Theory]
        [InlineData("CWE-79")]
        [InlineData("cwe-79")]
        [InlineData("Cwe-79")]
        [InlineData("  CWE-79  ")]
        public void Enrich_MatchesCanonicalCweIdsCaseInsensitive(string id)
        {
            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor { Id = id },
                        },
                    },
                },
            };

            int modified = CweTaxonomyEnricher.Enrich(run);

            modified.Should().Be(1);
            run.Tool.Driver.Rules[0].Id.Should().Be(id);
            run.Tool.Driver.Rules[0].Name.Should().Be("CrossSiteScripting");
        }

        [Theory]
        [InlineData("CWE_79")]
        [InlineData("CWE79")]
        [InlineData("79")]
        [InlineData("CWE-79/api-handler")]
        [InlineData("CWE-")]
        [InlineData("CWE-7a")]
        [InlineData("not-a-cwe")]
        [InlineData("MYTOOL1001")]
        public void Enrich_SkipsNonCanonicalCweIds(string id)
        {
            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor { Id = id },
                        },
                    },
                },
            };

            int modified = CweTaxonomyEnricher.Enrich(run);

            modified.Should().Be(0);
            run.Tool.Driver.Rules[0].Name.Should().BeNull();
        }

        [Fact]
        public void Enrich_PopulatesEmptyDescriptorFromTaxonomy()
        {
            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = "TestProducer",
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor { Id = "CWE-79" },
                        },
                    },
                },
            };

            int modified = CweTaxonomyEnricher.Enrich(run);

            modified.Should().Be(1);
            ReportingDescriptor rule = run.Tool.Driver.Rules[0];
            rule.Name.Should().Be("CrossSiteScripting");
            rule.ShortDescription.Text.Should().NotBeNullOrWhiteSpace();
            rule.HelpUri.OriginalString.Should().Be("https://cwe.mitre.org/data/definitions/79.html");
            rule.Help.Markdown.Should().Contain("## Description");
        }

        [Fact]
        public void Enrich_PreservesProducerSuppliedFields()
        {
            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = "TestProducer",
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor
                            {
                                Id = "CWE-79",
                                Name = "MyCustomName",
                                ShortDescription = new MultiformatMessageString { Text = "My custom short description." },
                                HelpUri = new Uri("https://example.com/my-rule"),
                            },
                        },
                    },
                },
            };

            CweTaxonomyEnricher.Enrich(run);

            ReportingDescriptor rule = run.Tool.Driver.Rules[0];
            rule.Name.Should().Be("MyCustomName");
            rule.ShortDescription.Text.Should().Be("My custom short description.");
            rule.HelpUri.OriginalString.Should().Be("https://example.com/my-rule");

            // Fields the producer left empty get populated.
            rule.FullDescription.Should().NotBeNull();
            rule.Help.Should().NotBeNull();
        }

        [Fact]
        public void Enrich_WalksExtensionRules()
        {
            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent { Name = "TestProducer" },
                    Extensions = new List<ToolComponent>
                    {
                        new ToolComponent
                        {
                            Name = "AnExtension",
                            Rules = new List<ReportingDescriptor>
                            {
                                new ReportingDescriptor { Id = "CWE-79" },
                            },
                        },
                    },
                },
            };

            int modified = CweTaxonomyEnricher.Enrich(run);

            modified.Should().Be(1);
            run.Tool.Extensions[0].Rules[0].Name.Should().Be("CrossSiteScripting");
        }

        [Fact]
        public void Enrich_DefaultStatusesIncludeIncomplete()
        {
            // SSRF (CWE-918) is Incomplete upstream but in the default loadout.
            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor { Id = "CWE-918" },
                        },
                    },
                },
            };

            int modified = CweTaxonomyEnricher.Enrich(run);
            modified.Should().Be(1);
            run.Tool.Driver.Rules[0].Name.Should().NotBeNullOrEmpty();
            run.Tool.Driver.Rules[0].HelpUri.OriginalString.Should().Be("https://cwe.mitre.org/data/definitions/918.html");
        }

        [Fact]
        public void Enrich_DefaultStatusesExcludeDeprecated()
        {
            // The enricher gives no help on Deprecated CWEs by default — the descriptor is left
            // empty so the producer notices the migration signal. Opting into CweStatus.Deprecated
            // (or CweStatus.All) enables enrichment.
            string deprecatedId = CweTaxonomy.Load(CweStatus.Deprecated).Runs[0].Taxonomies[0].Taxa[0].Id;

            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor { Id = deprecatedId },
                        },
                    },
                },
            };

            int defaultPass = CweTaxonomyEnricher.Enrich(run);
            defaultPass.Should().Be(0);
            run.Tool.Driver.Rules[0].Name.Should().BeNull();

            int allPass = CweTaxonomyEnricher.Enrich(run, CweStatus.All);
            allPass.Should().Be(1);
            run.Tool.Driver.Rules[0].Name.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Enrich_NullRun_Throws()
        {
            Action act = () => CweTaxonomyEnricher.Enrich(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Enrich_DerivesShortDescriptionWhenTaxonOmitsIt()
        {
            // CWE-89's taxon omits shortDescription because it is recoverable from the
            // first sentence of fullDescription (SARIF §3.49.10). The enricher derives it.
            ReportingDescriptor taxon = LoadTaxon("CWE-89");
            taxon.ShortDescription.Should().BeNull("the taxonomy omits short when it is the first sentence of full");

            ReportingDescriptor rule = EnrichSingleRule("CWE-89");

            rule.FullDescription.Text.Should().NotBeNullOrWhiteSpace();
            rule.ShortDescription.Text.Should().NotBeNullOrWhiteSpace();
            rule.FullDescription.Text.Should().StartWith(rule.ShortDescription.Text);
            rule.ShortDescription.Text.Should().EndWith(".");
            rule.ShortDescription.Text.Should().NotBe(rule.FullDescription.Text, "full carries additional sentences");
        }

        [Fact]
        public void Enrich_CopiesTaxonSuppliedShortDescriptionVerbatim()
        {
            // CWE-23's taxon retains shortDescription because its first sentence is not
            // recoverable by the dead-simple rule; the enricher copies it verbatim.
            ReportingDescriptor taxon = LoadTaxon("CWE-23");
            taxon.ShortDescription.Text.Should().NotBeNullOrWhiteSpace("this taxon keeps an explicit short");

            ReportingDescriptor rule = EnrichSingleRule("CWE-23");

            rule.ShortDescription.Text.Should().Be(taxon.ShortDescription.Text);
        }

        [Fact]
        public void Enrich_PreservesProducerShortWhenTaxonOmitsIt()
        {
            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor
                            {
                                Id = "CWE-89",
                                ShortDescription = new MultiformatMessageString { Text = "Producer short." },
                            },
                        },
                    },
                },
            };

            CweTaxonomyEnricher.Enrich(run);

            run.Tool.Driver.Rules[0].ShortDescription.Text.Should().Be("Producer short.");
        }

        private static ReportingDescriptor EnrichSingleRule(string id)
        {
            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor { Id = id },
                        },
                    },
                },
            };

            CweTaxonomyEnricher.Enrich(run);
            return run.Tool.Driver.Rules[0];
        }

        private static ReportingDescriptor LoadTaxon(string id)
        {
            foreach (ReportingDescriptor entry in CweTaxonomy.Load().Runs[0].Taxonomies[0].Taxa)
            {
                if (entry.Id == id) { return entry; }
            }

            throw new InvalidOperationException($"Taxon {id} not found in default loadout.");
        }
    }
}
