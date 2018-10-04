// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Tests
{
    public class SarifUtilitiesTests
    {
        [Fact]
        public void ConvertToSchemaUriTestV100()
        {
            Uri uri = SarifVersion.OneZeroZero.ConvertToSchemaUri();
            Assert.Equal(SarifUtilities.SarifSchemaUriBase + SarifUtilities.V1_0_0, uri.ToString());
        }

        [Fact]
        public void ConvertToSchemaUriTestVCurrent()
        {
            Uri uri = SarifVersion.Current.ConvertToSchemaUri();
            Assert.Equal(SarifUtilities.SarifSchemaUri, uri.ToString());
        }
    }
}
