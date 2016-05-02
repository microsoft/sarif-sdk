// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    [TestClass]
    public class ClangAnalyzerConverterTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClangAnalyzerConverter_Convert_NullInput()
        {
            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            converter.Convert(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClangAnalyzerConverter_Convert_NullOutput()
        {
            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            converter.Convert(new MemoryStream(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClangAnalyzerConverter_Convert_NullLogTest()
        {
            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            converter.Convert(null, new ResultLogObjectWriter());
        }

        private const string empty = @"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0-beta.4"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""Clang""
      },
      ""results"": []
    }
  ]
}";

        [TestMethod]
        public void ClangAnalyzerConverter_Convert_LogEmptyPlist()
        {
            string clangAnalyzerLog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"> <plist version=\"1.0\"></plist>";
            RunClangTestCase(clangAnalyzerLog, empty);
        }

        [TestMethod]
        public void ClangAnalyzerConverter_Convert_LogEmptyWithVersion()
        {
            string clangAnalyzerLog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"> <plist version=\"1.0\"> <dict> <key>clang_version</key> <string>Ubuntu clang version 3.4-1ubuntu3 (tags/RELEASE_34/final) (based on LLVM 3.4)</string> <key>files</key> <array> <string>jmemmgr.c</string> </array> </dict></plist>";
            RunClangTestCase(clangAnalyzerLog, empty);
        }

        [TestMethod]
        public void ClangAnalyzerConverter_Convert_LogEmptyWithWhitespace()
        {
            string clangAnalyzerLog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n<plist version=\"1.0\">\r\n<dict>\r\n<key>clang_version</key>\r\n<string>Ubuntu clang version 3.4-1ubuntu3 (tags/RELEASE_34/final) (based on LLVM 3.4)</string>\r\n<key>files</key>\r\n<array>\r\n</array>\r\n<key>diagnostics</key>\r\n<array>\r\n</array>\r\n</dict>\r\n</plist>\r\n";
            RunClangTestCase(clangAnalyzerLog, empty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void ClangAnalyzerConverter_BadIntValue()
        {
            string clangAnalyzerLog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n<plist version=\"1.0\">\r\n<dict>\r\n<key>clang_version</key>\r\n<string>Ubuntu clang version 3.4-1ubuntu3 (tags/RELEASE_34/final) (based on LLVM 3.4)</string>\r\n<key>files</key>\r\n<array>\r\n<string>jcparam.c</string>\r\n</array>\r\n<key>diagnostics</key>\r\n<array>\r\n<dict>\r\n<key>path</key>\r\n<array>\r\n<dict>\r\n<key>kind</key>\r\n<string>event</string>\r\n<key>location</key>\r\n<dict>\r\n<key>line</key>\r\n<integer>Bogus</integer>\r\n<key>col</key>\r\n<integer>5</integer>\r\n<key>file</key>\r\n<integer>0</integer>\r\n</dict>\r\n<key>ranges</key>\r\n<array>\r\n<array>\r\n<dict>\r\n<key>line</key>\r\n<integer>595</integer>\r\n<key>col</key>\r\n<integer>15</integer>\r\n<key>file</key>\r\n<integer>0</integer>\r\n</dict>\r\n<dict>\r\n<key>line</key>\r\n<integer>595</integer>\r\n<key>col</key>\r\n<integer>50</integer>\r\n<key>file</key>\r\n<integer>0</integer>\r\n</dict>\r\n</array>\r\n</array>\r\n<key>depth</key>\r\n<integer>0</integer>\r\n<key>extended_message</key>\r\n<string>Value stored to &apos;scanptr&apos; is never read</string>\r\n<key>message</key>\r\n<string>Value stored to &apos;scanptr&apos; is never read</string>\r\n</dict>\r\n</array>\r\n<key>description</key>\r\n<string>Value stored to &apos;scanptr&apos; is never read</string>\r\n<key>category</key>\r\n<string>Dead store</string>\r\n<key>type</key>\r\n<string>Dead assignment</string>\r\n<key>issue_context_kind</key>\r\n<string>function</string>\r\n<key>issue_context</key>\r\n<string>jpeg_simple_progression</string>\r\n<key>issue_hash</key>\r\n<string>57</string>\r\n<key>location</key>\r\n<dict>\r\n<key>line</key>\r\n<integer>Bogus</integer>\r\n<key>col</key>\r\n<integer>5</integer>\r\n<key>file</key>\r\n<integer>0</integer>\r\n</dict>\r\n<key>HTMLDiagnostics_files</key>\r\n<array>\r\n<string>report-ab0d45.html</string>\r\n</array>\r\n</dict>\r\n</array>\r\n</dict>\r\n</plist>\r\n";

            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            Utilities.GetConverterJson(converter, clangAnalyzerLog);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void ClangAnalyzerConverter_MissingStringDictionaryKey()
        {
            string clangAnalyzerLog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n<plist version=\"1.0\">\r\n<dict>\r\n<string>No Key Throw</string>\r\n<key>files</key>\r\n<array>\r\n<string>jcparam.c</string>\r\n</array>\r\n</dict>\r\n</plist>\r\n";

            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            Utilities.GetConverterJson(converter, clangAnalyzerLog);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void ClangAnalyzerConverter_MissingArrayDictionaryKey()
        {
            string clangAnalyzerLog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n<plist version=\"1.0\">\r\n<dict>\r\n<array></array>\r\n<key>files</key>\r\n<array>\r\n<string>jcparam.c</string>\r\n</array>\r\n</dict>\r\n</plist>\r\n";

            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            Utilities.GetConverterJson(converter, clangAnalyzerLog);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void ClangAnalyzerConverter_MissingNestedStringDictionaryKey()
        {
            string clangAnalyzerLog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n<plist version=\"1.0\">\r\n<dict>\r\n<key>clang_version</key>\r\n<string>Ubuntu clang version 3.4-1ubuntu3 (tags/RELEASE_34/final) (based on LLVM 3.4)</string>\r\n<key>files</key>\r\n<array>\r\n<string>jcparam.c</string>\r\n</array>\r\n<key>diagnostics</key>\r\n<array>\r\n<dict>\r\n<string>\r\n</string>\r\n</dict>\r\n</array>\r\n</dict>\r\n</plist>\r\n";
            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            Utilities.GetConverterJson(converter, clangAnalyzerLog);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void ClangAnalyzerConverter_MissingNestedArrayDictionaryKey()
        {
            string clangAnalyzerLog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n<plist version=\"1.0\">\r\n<dict>\r\n<key>clang_version</key>\r\n<string>Ubuntu clang version 3.4-1ubuntu3 (tags/RELEASE_34/final) (based on LLVM 3.4)</string>\r\n<key>files</key>\r\n<array>\r\n<string>jcparam.c</string>\r\n</array>\r\n<key>diagnostics</key>\r\n<array>\r\n<dict>\r\n<array>\r\n</array>\r\n</dict>\r\n</array>\r\n</dict>\r\n</plist>\r\n";
            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            Utilities.GetConverterJson(converter, clangAnalyzerLog);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void ClangAnalyzerConverter_MissingNestedDictionaryKey()
        {
            string clangAnalyzerLog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n<plist version=\"1.0\">\r\n<dict>\r\n<key>clang_version</key>\r\n<string>Ubuntu clang version 3.4-1ubuntu3 (tags/RELEASE_34/final) (based on LLVM 3.4)</string>\r\n<key>files</key>\r\n<array>\r\n<string>jcparam.c</string>\r\n</array>\r\n<key>diagnostics</key>\r\n<array>\r\n<dict>\r\n<dict>\r\n</dict>\r\n</dict>\r\n</array>\r\n</dict>\r\n</plist>\r\n";
            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            Utilities.GetConverterJson(converter, clangAnalyzerLog);
        }

        private void RunClangTestCase(string inputData, string expectedResult)
        {
            ClangAnalyzerConverter converter = new ClangAnalyzerConverter();
            string actualJson = Utilities.GetConverterJson(converter, inputData);
            Assert.AreEqual<string>(expectedResult, actualJson);
        }
    }
}






