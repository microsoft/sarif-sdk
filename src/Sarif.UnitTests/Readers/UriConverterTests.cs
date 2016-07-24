// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Readers.UnitTests
{
    [TestClass]
    public class UriConverterTests : JsonTests
    {
        [TestMethod]
        public void ConvertsHttpUri()
        {
            TestConverter("http://www.example.com/dir/file.c", "http://www.example.com/dir/file.c");
        }

        [TestMethod]
        public void ConvertsHttpUriWithEscaping()
        {
            TestConverter("http://www.example.com/dir/file name.c", "http://www.example.com/dir/file%20name.c");
        }

        [TestMethod]
        public void ConvertsAbsoluteWindowsFilePath()
        {
            TestConverter(@"C:\dir\file.c", "file:///C:/dir/file.c");
        }

        [TestMethod]
        public void ConvertsAbsoluteWindowsFilePathWithEscaping()
        {
            TestConverter(@"C:\dir\file name.c", "file:///C:/dir/file%20name.c");
        }

        [TestMethod]
        public void ConvertsAbsoluteUnixFilePath()
        {
            TestConverter("/dir/file.c", "/dir/file.c");
        }

        [TestMethod]
        public void ConvertsAbsoluteUnixFilePathWithEscaping()
        {
            TestConverter("/dir/file name.c", "/dir/file%20name.c");
        }

        [TestMethod]
        public void ConvertsAbsoluteWindowsFileUri()
        {
            TestConverter("file:///C:/dir/file.c", "file:///C:/dir/file.c");
        }

        [TestMethod]
        public void ConvertsAbsoluteWindowsFileUriWithEscaping()
        {
            TestConverter("file:///C:/dir/file name.c", "file:///C:/dir/file%20name.c");
        }

        [TestMethod]
        public void ConvertsRelativeWindowsFilePath()
        {
            TestConverter(@"dir\file.c", "dir/file.c");
        }

        [TestMethod]
        public void ConvertsRelativeWindowsFilePathWithEscaping()
        {
            TestConverter(@"dir\file name.c", "dir/file%20name.c");
        }

        [TestMethod]
        public void ConvertsRelativeUnixFilePath()
        {
            TestConverter("dir/file.c", "dir/file.c");
        }

        [TestMethod]
        public void ConvertsRelativeUnixFilePathWithEscaping()
        {
            TestConverter("dir/file name.c", "dir/file%20name.c");
        }

        [TestMethod]
        public void ConvertsRelativePathWithDotSegments()
        {
            TestConverter(@"..\..\.\.\..\dir1\dir2\file.c", "../../../dir1/dir2/file.c");
        }

        private void TestConverter(string inputUri, string expectedUri)
        {
            string expectedOutput =
@"{
  ""$schema"": """ + SarifSchemaUri + @""",
  ""version"": """ + SarifFormatVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""results"": [
        {
          ""locations"": [
            {
              ""analysisTarget"": {
                ""uri"": """ + expectedUri + @"""
              }
            }
          ]
        }
      ]
    }
  ]
}";
            string actualOutput = GetJson(uut =>
            {
                var run = new Run();

                uut.Initialize(id: null, correlationId: null);

                uut.WriteTool(DefaultTool);

                var result = new Result
                {
                    Locations = new List<Location>
                    {
                        new Location
                        {
                            AnalysisTarget = new PhysicalLocation
                            {
                                Uri = new Uri(inputUri, UriKind.RelativeOrAbsolute)
                            }
                        }
                    }
                };

                uut.WriteResults(new[] { result });
            });

            Assert.AreEqual(expectedOutput, actualOutput);
        }
    }
}
