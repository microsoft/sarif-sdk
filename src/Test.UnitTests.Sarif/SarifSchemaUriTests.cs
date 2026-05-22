// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifSchemaUriTests
    {
        [Fact]
        public void SarifSchemaUri_ResolvesToFinalV210SchemaUri()
        {
            SarifUtilities.SarifSchemaUri
                .Should().Be("https://schemastore.azurewebsites.net/schemas/json/sarif-2.1.0.json");
        }

        [Fact]
        public void SarifSchemaUri_DoesNotIncludePrereleaseSuffix()
        {
            SarifUtilities.SarifSchemaUri.Should().NotContain("-rtm");
            SarifUtilities.SarifSchemaUri.Should().NotContain("-beta");
            SarifUtilities.SarifSchemaUri.Should().NotContain("-csd");
        }

        [Fact]
        public void FinalV210SchemaUri_MatchesSarifSchemaUri()
        {
            SarifUtilities.FinalV210SchemaUri.Should().Be(SarifUtilities.SarifSchemaUri);
        }

        [Fact]
        public void ConvertToSchemaUri_Current_ReturnsFinalUrl()
        {
            SarifVersion.Current.ConvertToSchemaUri().OriginalString
                .Should().Be(SarifUtilities.FinalV210SchemaUri);
        }

        [Fact]
        public void ConvertToSchemaUri_OneZeroZero_StillComposesViaBase()
        {
            SarifVersion.OneZeroZero.ConvertToSchemaUri().OriginalString
                .Should().Be(SarifUtilities.SarifSchemaUriBase + SarifUtilities.V1_0_0 + ".json");
        }
    }
}
