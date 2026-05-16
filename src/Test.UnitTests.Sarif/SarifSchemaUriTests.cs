// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    /// <summary>
    /// Tests covering SDK-H: <see cref="SarifUtilities.SarifSchemaUri"/> now points at the
    /// final v2.1.0 schema URL on the <c>schemastore.azurewebsites.net</c> host with no
    /// <c>-rtm</c> prerelease suffix. The host is the canonical Microsoft-emitted alias
    /// (MSVC <c>/analyze</c>, microsoft/sarif-tutorials, microsoft/sarif-vscode-extension all
    /// reference it); it 301-redirects to <c>www.schemastore.org</c>, which is the public
    /// JSON Schema Store catalog. Both routes serve the same content.
    /// </summary>
    public class SarifSchemaUriTests
    {
        private const string ExpectedFinalSchemaUri =
            "https://schemastore.azurewebsites.net/schemas/json/sarif-2.1.0.json";

        [Fact]
        public void SarifSchemaUri_IsFinalSchemastoreUrl()
        {
            SarifUtilities.SarifSchemaUri.Should().Be(ExpectedFinalSchemaUri);
        }

        [Fact]
        public void SarifSchemaUri_DoesNotPointAtPrereleaseAlias()
        {
            SarifUtilities.SarifSchemaUri.Should().NotContain("rtm.",
                "the final v2.1.0 URL must not carry a '-rtm' prerelease suffix");
        }

        [Fact]
        public void ConvertToSchemaUri_Current_ReturnsFinalSchemastoreUrl()
        {
            SarifVersion.Current.ConvertToSchemaUri().OriginalString.Should().Be(ExpectedFinalSchemaUri);
        }

        [Fact]
        public void ConvertToSchemaUri_v1_0_0_StillResolvesUnderLegacyBase()
        {
            // v1.0.0 has no OASIS "final" equivalent, so it continues to resolve under the
            // legacy schemastore base. Pinning this here so the v1 path doesn't silently
            // change if SarifSchemaUriBase is ever reshaped.
            SarifVersion.OneZeroZero.ConvertToSchemaUri().OriginalString.Should()
                .Be(SarifUtilities.SarifSchemaUriBase + SarifUtilities.V1_0_0 + ".json");
        }
    }
}
