// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    /// <summary>
    /// Tests covering SDK-H: <see cref="SarifUtilities.SarifSchemaUri"/> now points at the
    /// OASIS-published v2.1.0 errata01 final schema URL. The previous schemastore.azurewebsites.net
    /// /sarif-2.1.0-rtm.6.json variant was a rolling-prerelease alias that the SARIF Multitool
    /// validator does NOT accept as final.
    /// </summary>
    public class SarifSchemaUriTests
    {
        private const string ExpectedFinalSchemaUri =
            "https://docs.oasis-open.org/sarif/sarif/v2.1.0/errata01/os/schemas/sarif-schema-2.1.0.json";

        [Fact]
        public void SarifSchemaUri_IsOasisFinal()
        {
            SarifUtilities.SarifSchemaUri.Should().Be(ExpectedFinalSchemaUri);
        }

        [Fact]
        public void SarifSchemaUri_DoesNotPointAtSchemastoreVariant()
        {
            SarifUtilities.SarifSchemaUri.Should().NotContain("schemastore.azurewebsites.net");
            SarifUtilities.SarifSchemaUri.Should().NotContain("rtm.");
        }

        [Fact]
        public void ConvertToSchemaUri_Current_ReturnsOasisFinal()
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
