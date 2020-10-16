// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Xml;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class CppCheckConverterTests : ConverterTestsBase<CppCheckConverter>
    {
        [Fact]
        public void CppCheckConverter_Convert_NullInput()
        {
            CppCheckConverter converter = new CppCheckConverter();
            Assert.Throws<ArgumentNullException>(() => converter.Convert(null, null, OptionallyEmittedData.None));
        }

        [Fact]
        public void CppCheckConverter_Convert_NullOutput()
        {
            CppCheckConverter converter = new CppCheckConverter();
            Assert.Throws<ArgumentNullException>(() => converter.Convert(new MemoryStream(), null, OptionallyEmittedData.None));
        }

        [Fact]
        public void CppCheckConverter_Convert_NullLogTest()
        {
            CppCheckConverter converter = new CppCheckConverter();
            Assert.Throws<ArgumentNullException>(() => converter.Convert(null, new ResultLogObjectWriter(), OptionallyEmittedData.None));
        }

        [Fact]
        public void CppCheckConverter_ExtractsCppCheckVersion()
        {
            ResultLogObjectWriter results = Utilities.GetConverterObjects(new CppCheckConverter(),
                "<results> <cppcheck version=\"12.34\" /> <errors /> </results>");

            // We will transform the version above to a Semantic Versioning 2.0 form
            Assert.Equal("12.34.0", results.Run.Tool.Driver.Version);
        }

        [Fact]
        public void CppCheckConverter_HandlesEmptyErrorsElement()
        {
            const string source = "<results> <cppcheck version=\"12.34\" /> <errors>   </errors> </results>";

            SarifLog emptyLog = EmptyLog.DeepClone();
            emptyLog.Runs[0].Tool.Driver.Version = "12.34.0";
            RunTestCase(source, JsonConvert.SerializeObject(emptyLog));
        }

        [Fact]
        public void CppCheckConverter_Invalid_RootNodeNotResults()
        {
            Assert.Throws<XmlException>(() => Utilities.GetConverterJson(new CppCheckConverter(), "<bad_root_node />"));
        }

        [Fact]
        public void CppCheckConverter_Invalid_FirstFollowingNodeNotCppCheck()
        {
            Assert.Throws<XmlException>(() => Utilities.GetConverterJson(new CppCheckConverter(), "<results> <a_different_node /> </results>"));
        }

        [Fact]
        public void CppCheckConverter_Invalid_MissingErrorsElement()
        {
            Assert.Throws<XmlException>(() => Utilities.GetConverterJson(new CppCheckConverter(), "<results> <cppcheck version=\"12.34\" /> </results>"));
        }

        [Fact]
        public void CppCheckConverter_Invalid_MissingVersion()
        {
            Assert.Throws<XmlException>(() => Utilities.GetConverterJson(new CppCheckConverter(), "<results> <cppcheck /> <errors /> </results>"));
        }
    }
}
