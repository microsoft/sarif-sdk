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
            Assert.Equal("http://json.schemastore.org/sarif-1.0.0", uri.ToString());
        }

        [Fact]
        public void ConvertToSchemaUriTestV100Beta5()
        {
            Uri uri = SarifVersion.OneZeroZeroBetaFive.ConvertToSchemaUri();
            Assert.Equal("http://json.schemastore.org/sarif-1.0.0-beta.5", uri.ToString());
        }
    }
}
