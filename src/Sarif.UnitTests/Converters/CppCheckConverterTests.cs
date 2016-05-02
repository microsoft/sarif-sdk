// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    [TestClass]
    public class CppCheckConverterTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CppCheckConverter_Convert_NullInput()
        {
            CppCheckConverter converter = new CppCheckConverter();
            converter.Convert(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CppCheckConverter_Convert_NullOutput()
        {
            CppCheckConverter converter = new CppCheckConverter();
            converter.Convert(new MemoryStream(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CppCheckConverter_Convert_NullLogTest()
        {
            CppCheckConverter converter = new CppCheckConverter();
            converter.Convert(null, new ResultLogObjectWriter());
        }

        [TestMethod]
        public void CppCheckConverter_ExtractsCppCheckVersion()
        {
            ResultLogObjectWriter results = Utilities.GetConverterObjects(new CppCheckConverter(),
                "<results> <cppcheck version=\"12.34\" /> <errors /> </results>");

            // We will transform the version above to a Semantic Versioning 2.0 form
            Assert.AreEqual("12.34.0", results.Tool.Version);
        }

        [TestMethod]
        public void CppCheckConverter_HandlesEmptyErrorsElement()
        {
            const string source = "<results> <cppcheck version=\"12.34\" /> <errors>   </errors> </results>";
            const string expected = @"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0-beta.4"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CppCheck"",
        ""version"": ""12.34.0""
      },
      ""results"": []
    }
  ]
}";
            string resultJson = Utilities.GetConverterJson(new CppCheckConverter(), source);
            Assert.AreEqual(expected, resultJson);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckConverter_Invalid_RootNodeNotResults()
        {
            Utilities.GetConverterJson(new CppCheckConverter(), "<bad_root_node />");
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckConverter_Invalid_FirstFollowingNodeNotCppCheck()
        {
            Utilities.GetConverterJson(new CppCheckConverter(), "<results> <a_different_node /> </results>");
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckConverter_Invalid_MissingErrorsElement()
        {
            Utilities.GetConverterJson(new CppCheckConverter(), "<results> <cppcheck version=\"12.34\" /> </results>");
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckConverter_Invalid_MissingVersion()
        {
            Utilities.GetConverterJson(new CppCheckConverter(), "<results> <cppcheck /> <errors /> </results>");
        }
    }
}
