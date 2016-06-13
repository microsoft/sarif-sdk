// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Tests
{
    [TestClass]
    public class SarifUtilitiesTests
    {
        [TestMethod]
        public void ConvertToSchemaUriTestV100()
        {
            Uri uri = SarifVersion.OneZeroZero.ConvertToSchemaUri();
            Assert.AreEqual(uri.ToString(), "http://json.schemastore.org/sarif-1.0.0");
        }

        [TestMethod]
        public void ConvertToSchemaUriTestV100Beta5()
        {
            Uri uri = SarifVersion.OneZeroZeroBetaFive.ConvertToSchemaUri();
            Assert.AreEqual(uri.ToString(), "http://json.schemastore.org/sarif-1.0.0-beta.5");
        }
    }
}
