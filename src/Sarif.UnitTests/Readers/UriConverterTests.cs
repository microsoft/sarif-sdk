// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.TestUtilities;
using Newtonsoft.Json;
using Xunit;

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
            string expected = CreateCurrentV2SarifLogText(
                resultCount: 1,
                (log) => {
                    log.Runs[0].Results[0].Locations = new List<Location>
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    Uri = new Uri(expectedUri, UriKind.RelativeOrAbsolute)
                                }
                            }
                        }
                    };
                });
            
            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);

                var result = new Result
                {
                    Message = new Message {  Text = "Some testing occurred."},
                    Locations = new List<Location>
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    Uri = new Uri(inputUri, UriKind.RelativeOrAbsolute)
                                }
                            }
                        }
                    }
                };

                uut.WriteResults(new[] { result });
            });

            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);
        }
    }
}
