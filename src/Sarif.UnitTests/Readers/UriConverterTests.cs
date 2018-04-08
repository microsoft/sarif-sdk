// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Readers.UnitTests
{
    public class UriConverterTests : JsonTests
    {
        [Fact]
        public void ConvertsHttpUri()
        {
            TestConverter("http://www.example.com/dir/file.c", "http://www.example.com/dir/file.c");
        }

        [Fact]
        public void ConvertsHttpUriWithEscaping()
        {
            TestConverter("http://www.example.com/dir/file name.c", "http://www.example.com/dir/file%20name.c");
        }

        [Fact]
        public void ConvertsAbsoluteWindowsFilePath()
        {
            TestConverter(@"C:\dir\file.c", "file:///C:/dir/file.c");
        }

        [Fact]
        public void ConvertsAbsoluteWindowsFilePathWithEscaping()
        {
            TestConverter(@"C:\dir\file name.c", "file:///C:/dir/file%20name.c");
        }

        [Fact]
        public void ConvertsAbsoluteUnixFilePath()
        {
            TestConverter("/dir/file.c", "/dir/file.c");
        }

        [Fact]
        public void ConvertsAbsoluteUnixFilePathWithEscaping()
        {
            TestConverter("/dir/file name.c", "/dir/file%20name.c");
        }

        [Fact]
        public void ConvertsAbsoluteWindowsFileUri()
        {
            TestConverter("file:///C:/dir/file.c", "file:///C:/dir/file.c");
        }

        [Fact]
        public void ConvertsAbsoluteWindowsFileUriWithEscaping()
        {
            TestConverter("file:///C:/dir/file name.c", "file:///C:/dir/file%20name.c");
        }

        [Fact]
        public void ConvertsRelativeWindowsFilePath()
        {
            TestConverter(@"dir\file.c", "dir/file.c");
        }

        [Fact]
        public void ConvertsRelativeWindowsFilePathWithEscaping()
        {
            TestConverter(@"dir\file name.c", "dir/file%20name.c");
        }

        [Fact]
        public void ConvertsRelativeUnixFilePath()
        {
            TestConverter("dir/file.c", "dir/file.c");
        }

        [Fact]
        public void ConvertsRelativeUnixFilePathWithEscaping()
        {
            TestConverter("dir/file name.c", "dir/file%20name.c");
        }

        [Fact]
        public void ConvertsRelativePathWithDotSegments()
        {
            TestConverter(@"..\..\.\.\..\dir1\dir2\file.c", "../../../dir1/dir2/file.c");
        }

        [Fact]
        public void ConvertPathWithOnlyDotSegments()
        {
            TestConverter(@"..\..", "../..");
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
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": """ + expectedUri + @"""
                }
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

                uut.Initialize(id: null, automationId: null);

                uut.WriteTool(DefaultTool);

                var result = new Result
                {
                    Locations = new List<Location>
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                FileLocation = new FileLocation
                                {
                                    Uri = new Uri(inputUri, UriKind.RelativeOrAbsolute)
                                }
                            }
                        }
                    }
                };

                uut.WriteResults(new[] { result });
            });

            actualOutput.Should().BeCrossPlatformEquivalent(expectedOutput);
        }
    }
}
