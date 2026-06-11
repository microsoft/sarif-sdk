// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Taxonomies;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Test.UnitTests.Taxonomies
{
    public class CweSecuritySeverityTests
    {
        // ----- Numeric lookup (house style: [Fact] + shared helper) -----

        private static void VerifyNumericLookup(int cweNumber, bool expectedFound, double expectedValue)
        {
            bool found = CweSecuritySeverity.TryGetSecuritySeverity(cweNumber, out double value);

            found.Should().Be(expectedFound);
            if (expectedFound)
            {
                value.Should().Be(expectedValue);
            }
        }

        [Fact]
        public void TryGetSecuritySeverity_Int_ReturnsCuratedValue_ForKnownCwe()
        {
            VerifyNumericLookup(89, expectedFound: true, expectedValue: 8.8);
        }

        [Fact]
        public void TryGetSecuritySeverity_Int_ReturnsFalse_ForUnknownCwe()
        {
            VerifyNumericLookup(99999, expectedFound: false, expectedValue: default);
        }

        // ----- String lookup (accepts bare number, CWE-<n>, CWE-<n>/sub-id) -----

        private static void VerifyStringLookup(string cweId, bool expectedFound, double expectedValue)
        {
            bool found = CweSecuritySeverity.TryGetSecuritySeverity(cweId, out double value);

            found.Should().Be(expectedFound);
            if (expectedFound)
            {
                value.Should().Be(expectedValue);
            }
        }

        [Fact]
        public void TryGetSecuritySeverity_String_AcceptsBareNumber()
        {
            VerifyStringLookup("89", expectedFound: true, expectedValue: 8.8);
        }

        [Fact]
        public void TryGetSecuritySeverity_String_AcceptsCanonicalId()
        {
            VerifyStringLookup("CWE-89", expectedFound: true, expectedValue: 8.8);
        }

        [Fact]
        public void TryGetSecuritySeverity_String_AcceptsSubIdForm()
        {
            VerifyStringLookup("CWE-89/kql-injection", expectedFound: true, expectedValue: 8.8);
        }

        [Fact]
        public void TryGetSecuritySeverity_String_ToleratesCaseAndLeadingZeros()
        {
            VerifyStringLookup("cwe-089", expectedFound: true, expectedValue: 8.8);
        }

        [Fact]
        public void TryGetSecuritySeverity_String_ReturnsFalse_ForNovelForm()
        {
            VerifyStringLookup("NOVEL-prompt-injection", expectedFound: false, expectedValue: default);
        }

        [Fact]
        public void TryGetSecuritySeverity_String_ReturnsFalse_ForNullEmptyOrWhitespace()
        {
            VerifyStringLookup(null, expectedFound: false, expectedValue: default);
            VerifyStringLookup(string.Empty, expectedFound: false, expectedValue: default);
            VerifyStringLookup("   ", expectedFound: false, expectedValue: default);
        }

        // ----- Table integrity -----

        [Fact]
        public void SecuritySeverityByCwe_IsNonEmpty()
        {
            CweSecuritySeverity.SecuritySeverityByCwe.Should().NotBeEmpty();
        }

        [Fact]
        public void SecuritySeverityByCwe_AllValuesWithinCvssScale()
        {
            foreach (KeyValuePair<int, double> entry in CweSecuritySeverity.SecuritySeverityByCwe)
            {
                entry.Value.Should().BeInRange(0.0, 10.0, $"CWE-{entry.Key} must carry a 0.0-10.0 prior");
            }
        }

        // ----- Property-value formatting (the shape GHAS/GHAzDO read) -----

        [Fact]
        public void FormatPropertyValue_EmitsFixedOneDecimalInvariant()
        {
            CweSecuritySeverity.FormatPropertyValue(8.8).Should().Be("8.8");
            CweSecuritySeverity.FormatPropertyValue(10).Should().Be("10.0");
            CweSecuritySeverity.FormatPropertyValue(0).Should().Be("0.0");
        }
    }
}
