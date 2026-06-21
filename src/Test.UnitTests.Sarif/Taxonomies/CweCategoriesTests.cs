// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Taxonomies;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Test.UnitTests.Taxonomies
{
    public class CweCategoriesTests
    {
        // ----- TryGetCategoryName (house style: [Fact] + shared helper) -----

        private static void VerifyCategoryLookup(string cweId, bool expectedFound, string expectedName)
        {
            bool found = CweCategories.TryGetCategoryName(cweId, out string name);

            found.Should().Be(expectedFound, "for '{0}'", cweId);
            CweCategories.IsCategory(cweId).Should().Be(expectedFound, "for '{0}'", cweId);
            if (expectedFound)
            {
                name.Should().Be(expectedName);
            }
        }

        [Fact]
        public void TryGetCategoryName_ReturnsName_ForKnownCategory()
            => VerifyCategoryLookup("CWE-16", true, "Configuration");

        [Fact]
        public void TryGetCategoryName_ReturnsName_ForCategoryCarryingSubId()
            => VerifyCategoryLookup("CWE-16/insecure-default-config", true, "Configuration");

        [Fact]
        public void TryGetCategoryName_IsCaseInsensitiveOnPrefix()
            => VerifyCategoryLookup("cwe-16", true, "Configuration");

        [Fact]
        public void TryGetCategoryName_ReturnsFalse_ForWeakness()
            => VerifyCategoryLookup("CWE-89", false, null);

        [Fact]
        public void TryGetCategoryName_ReturnsFalse_ForView()
            => VerifyCategoryLookup("CWE-1000", false, null);

        [Fact]
        public void TryGetCategoryName_ReturnsFalse_ForNovelAndBareNumberAndNull()
        {
            VerifyCategoryLookup("NOVEL-prompt-injection", false, null);
            VerifyCategoryLookup("16", false, null);
            VerifyCategoryLookup(null, false, null);
        }

        [Fact]
        public void CategoryNamesByCwe_IsNonEmptyAndIncludesConfiguration()
        {
            CweCategories.CategoryNamesByCwe.Should().NotBeEmpty();
            CweCategories.CategoryNamesByCwe[16].Should().Be("Configuration");
        }
    }
}
