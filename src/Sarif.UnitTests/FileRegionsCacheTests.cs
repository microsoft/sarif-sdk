// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.ObjectModel;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    public class FileRegionsCacheTests
    {
        //                                 0          10          20         30
        //                                 01234 5 6789012 3 45678901 2 345679012
        private const string s_testText = "line1\r\n line2\r\n  line3\r\n   line4";

        private class TestCaseData
        {
            public TestCaseData(string snippet, Region inputRegion, Region outputRegion)
            {
                ExpectedSnippet = snippet;
                InputRegion = inputRegion;
                OutputRegion = outputRegion;
            }

            public string ExpectedSnippet { get; set; }
            public Region InputRegion { get; set; }
            public Region OutputRegion { get; set; }
        }

        private readonly static Region s_line1_NoNewlines =
            new Region() { StartLine = 1, EndLine = 1, StartColumn = 1, EndColumn = 6, CharOffset = 0, CharLength = 5 };

        private static ReadOnlyCollection<TestCaseData> s_testCases =
            new ReadOnlyCollection<TestCaseData>(new TestCaseData[]
            {
                // Regions specified only by start line
                new TestCaseData(
                    snippet: "line1",
                    inputRegion: new Region() { StartLine = 1 },
                    outputRegion: s_line1_NoNewlines)
            });

        [Fact]
        public void FileRegionsCache_PopulatesRegionsFromAbsoluteFileUri()
        {            
            var run = new Run();
            var fileRegionsCache = new FileRegionsCache(run);

            Uri uri = new Uri(@"c:\temp\myFile.cpp");
            var mockFileSystem = MockFactory.MakeMockFileSystem(uri.LocalPath, s_testText);

            fileRegionsCache._fileSystem = mockFileSystem;

            var physicalLocation = new PhysicalLocation()
            {
                FileLocation = new FileLocation()
                {
                    Uri = uri
                }
            };

            ExecuteTests(fileRegionsCache, physicalLocation);
        }

        private static void ExecuteTests(FileRegionsCache fileRegionsCache, PhysicalLocation physicalLocation)
        {
            foreach (TestCaseData testCase in s_testCases)
            {
                string snippet = testCase.ExpectedSnippet;
                Region inputRegion = testCase.InputRegion;
                Region expectedRegion = testCase.OutputRegion;

                physicalLocation.Region = inputRegion;

                Region actualRegion = fileRegionsCache.PopulatePrimaryRegionProperties(physicalLocation, populateSnippet: false);

                actualRegion.ValueEquals(expectedRegion).Should().BeTrue();
                actualRegion.Snippet.Should().BeNull();

                actualRegion = fileRegionsCache.PopulatePrimaryRegionProperties(physicalLocation, populateSnippet: true);

                expectedRegion.Snippet = new FileContent() { Text = snippet };
                actualRegion.ValueEquals(expectedRegion).Should().BeTrue();
                actualRegion.Snippet.Text.Should().Be(snippet);
            }
        }
    }
}
