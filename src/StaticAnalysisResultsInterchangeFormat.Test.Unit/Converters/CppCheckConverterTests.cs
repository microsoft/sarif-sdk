// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Writers;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Converters
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
            converter.Convert(null, new IssueLogObjectWriter());
        }

        [TestMethod]
        public void CppCheckConverter_ExtractsCppCheckVersion()
        {
            IssueLogObjectWriter results = Utilities.GetConverterObjects(new CppCheckConverter(),
                "<results> <cppcheck version=\"12.34\" /> <errors /> </results>");
            Assert.AreEqual("12.34", results.ToolInfo.ProductVersion);
        }

        [TestMethod]
        public void CppCheckConverter_HandlesEmptyErrorsElement()
        {
            const string source = "<results> <cppcheck version=\"12.34\" /> <errors>   </errors> </results>";
            const string expected = @"{
  ""version"": ""1.0"",
  ""toolInfo"": {
    ""toolName"": ""CppCheck"",
    ""productVersion"": ""12.34""
  },
  ""issues"": []
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
