using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.Tests
{
    [TestClass()]
    public class SarifUtilitiesTests
    {
        private const string V1_0_0 = "1.0.0";
        private const string V1_0_0_BETA_5 = "1.0.0-beta.5";

        [TestMethod()]
        public void ConvertToSchemaUriTest1()
        {
            V1_0_0.ConvertToSarifVersion().ConvertToSchemaUri();
        }

        [TestMethod()]
        public void ConvertToSchemaUriTest2()
        {
            V1_0_0_BETA_5.ConvertToSarifVersion().ConvertToSchemaUri();
        }
    }
}