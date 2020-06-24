// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    public class FileRegionsCacheTests
    {
        public FileRegionsCacheTests()
        {
            SarifUtilities.UnitTesting = true;
        }

        private class TestCaseData
        {
            public TestCaseData(Region inputRegion, Region outputRegion)
            {
                InputRegion = inputRegion;
                OutputRegion = outputRegion;
            }

            public string ExpectedSnippet { get; set; }
            public Region InputRegion { get; }
            public Region OutputRegion { get; }
        }

        //                                   0            10         19
        //                                   0123 4 5678 9 01234 5 6789 
        private const string SPEC_EXAMPLE = "abcd\r\nefg\r\nhijk\r\nlmn";


        // Breaking the lines for readability and per-line column details
        // 
        // Column: 123 4 5 6
        // Line 1: abc d\r\n
        //      2: efg\r\n
        //      3: hij k\r\n
        //      4: lmn

        private const string COMPLETE_FILE = SPEC_EXAMPLE;

        private const string LINE3 = "hijk";
        private const string NEW_LINE = "\n";
        private const string INSERTION_POINT = "";
        private const string CARRIAGE_RETURN = "\r";
        private const string LINE1_NO_NEWLINES = "abcd";
        private const string INTERIOR_CHARACTERS = "ij";
        private const string INTERIOR_NEWLINES = "\nefg\r";
        private const string LINES_2_AND_3 = "efg\r\nhijk";
        private const string CARRIAGE_RETURN_NEW_LINE = "\r\n";

        private readonly static Region s_Insertion_Beginning_Of_OffsetBased_Text_File =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = INSERTION_POINT },
                StartLine = 1,
                StartColumn = 1,
                EndLine = 1,
                EndColumn = 1,
                CharOffset = 0,
                CharLength = 0
            };

        private readonly static Region s_Insertion_Beginning_Of_LineColumnBased_Text_File =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = INSERTION_POINT },
                StartLine = 1,
                StartColumn = 1,
                EndLine = 1,
                EndColumn = 1,
                CharOffset = 0,
                CharLength = 0
            };

        private readonly static Region s_Insertion_Beginning_Of_Binary_File =
            new Region()
            {
                Snippet = null,
                StartLine = 0,
                StartColumn = 0,
                EndLine = 0,
                EndColumn = 0,
                CharOffset = -1,
                CharLength = 0,
                ByteOffset = 0,
                ByteLength = 0
            };

        private readonly static Region s_Insertion_End_Of_File =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = INSERTION_POINT },
                StartLine = 4,
                StartColumn = 4,
                EndLine = 4,
                EndColumn = 4,
                CharOffset = 20,
                CharLength = 0
            };

        private readonly static Region s_Insertion_Between_New_Line_Chars =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = INSERTION_POINT },
                StartLine = 2,
                StartColumn = 5,
                EndLine = 2,
                EndColumn = 5,
                CharOffset = 10,
                CharLength = 0
            };

        private readonly static Region s_Interior_New_Line =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = NEW_LINE },
                StartLine = 2,
                StartColumn = 5,
                EndLine = 3,
                EndColumn = 1,
                CharOffset = 10,
                CharLength = 1
            };

        private readonly static Region s_Interior_Carriage_Return =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = CARRIAGE_RETURN },
                StartLine = 2,
                StartColumn = 4,
                EndLine = 2,
                EndColumn = 5,
                CharOffset = 9,
                CharLength = 1
            };

        // Version 1 of this region defines it by using the insertion point of the following line as the terminus
        private readonly static Region s_Interior_Carriage_Return_New_Line_V1 =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = CARRIAGE_RETURN_NEW_LINE },
                StartLine = 3,
                StartColumn = 5,
                EndLine = 4,
                EndColumn = 1,
                CharOffset = 15,
                CharLength = 2
            };

        // Version 2 of this region defines it by using an endColumn value that extends past the actual line ending
        private readonly static Region s_Interior_Carriage_Return_New_Line_V2 =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = CARRIAGE_RETURN_NEW_LINE },
                StartLine = 3,
                StartColumn = 5,
                EndLine = 3,
                EndColumn = 7,
                CharOffset = 15,
                CharLength = 2
            };

        // Version 1 of this region defines it by using the insertion point of the following line as the terminus
        private readonly static Region s_Complete_File_V1 =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = COMPLETE_FILE },
                StartLine = 1,
                StartColumn = 1,
                EndLine = 5,
                EndColumn = 1,
                CharOffset = 0,
                CharLength = 20
            };

        // Version 2 of this region defines it by using an endColumn value that extends past the actual line ending
        private readonly static Region s_Complete_File_V2 =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = COMPLETE_FILE },
                StartLine = 1,
                StartColumn = 1,
                EndLine = 4,
                EndColumn = 4,
                CharOffset = 0,
                CharLength = 20
            };

        private readonly static Region s_Line_3 =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = LINE3 },
                StartLine = 3,
                StartColumn = 1,
                EndLine = 3,
                EndColumn = 5,
                CharOffset = 11,
                CharLength = 4
            };

        private readonly static Region s_Lines_2_And_3 =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = LINES_2_AND_3 },
                StartLine = 2,
                StartColumn = 1,
                EndLine = 3,
                EndColumn = 5,
                CharOffset = 6,
                CharLength = 9
            };

        private readonly static Region s_Interior_Characters =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = INTERIOR_CHARACTERS },
                StartLine = 3,
                StartColumn = 2,
                EndLine = 3,
                EndColumn = 4,
                CharOffset = 12,
                CharLength = 2
            };

        private const string COMPLETE_FILE_NEW_LINES_ONLY = "123\n456\n789\n";
        private const string FRAGMENT_NEW_LINES_ONLY = "\n456\n789\n";

        private const string COMPLETE_FILE_CARRIAGE_RETURNS_ONLY = "\r\r\r12\r345\r";
        private const string FRAGMENT_CARRIAGE_RETURNS_ONLY = "2\r345\r";

        private readonly static Region s_Complete_File_New_Lines_Only =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = COMPLETE_FILE_NEW_LINES_ONLY },
                StartLine = 1,
                StartColumn = 1,
                EndLine = 4,
                EndColumn = 1,
                CharOffset = 0,
                CharLength = 12
            };

        private readonly static Region s_Fragment_New_Lines_Only =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = FRAGMENT_NEW_LINES_ONLY },
                StartLine = 1,
                StartColumn = 4,
                EndLine = 4,
                EndColumn = 1,
                CharOffset = 3,
                CharLength = 9
            };

        private readonly static Region s_Complete_File_Carriage_Returns_Only =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = COMPLETE_FILE_CARRIAGE_RETURNS_ONLY },
                StartLine = 1,
                StartColumn = 1,
                EndLine = 6,
                EndColumn = 1,
                CharOffset = 0,
                CharLength = 10
            };

        private readonly static Region s_Fragment_Carriage_Returns_Only =
            new Region()
            {
                Snippet = new ArtifactContent() { Text = FRAGMENT_CARRIAGE_RETURNS_ONLY },
                StartLine = 4,
                StartColumn = 2,
                EndLine = 6,
                EndColumn = 1,
                CharOffset = 4,
                CharLength = 6
            };

        private static readonly ReadOnlyCollection<TestCaseData> s_specExampleTestCases =
            new ReadOnlyCollection<TestCaseData>(new TestCaseData[]
            {   
                // Insertion point at beginning of an offset based text file
                new TestCaseData(outputRegion : s_Insertion_Beginning_Of_OffsetBased_Text_File,
                    inputRegion: new Region() { CharOffset = 0}),

                // Insertion point at beginning of a line/column based text file, can only
                // be denoted by use of startLine
                new TestCaseData(outputRegion : s_Insertion_Beginning_Of_LineColumnBased_Text_File,
                    inputRegion: new Region() { StartLine = 1, StartColumn = 1, EndColumn = 1, CharOffset = 0 }),

                // Insertion point at beginning of a based binary file
                new TestCaseData(outputRegion : s_Insertion_Beginning_Of_Binary_File,
                    inputRegion: new Region() { ByteOffset = 0}),

                new TestCaseData(outputRegion : s_Insertion_End_Of_File,
                    inputRegion: new Region() { CharOffset = 20 }),

                new TestCaseData(outputRegion : s_Insertion_Between_New_Line_Chars,
                    inputRegion: new Region() { CharOffset = 10 }),

                new TestCaseData(outputRegion : s_Interior_Carriage_Return,
                    inputRegion: new Region() { CharOffset = 9, CharLength = 1 }),

                new TestCaseData(outputRegion : s_Interior_New_Line,
                    inputRegion: new Region() { CharOffset = 10, CharLength = 1 }),

                new TestCaseData(outputRegion : s_Interior_Carriage_Return_New_Line_V1,
                    inputRegion: new Region() { CharOffset = 15, CharLength = 2 }),

                new TestCaseData(outputRegion : s_Interior_Carriage_Return_New_Line_V1,
                    inputRegion: new Region() { StartLine = 3, StartColumn = 5, EndLine = 4, EndColumn = 1 }),

                new TestCaseData(outputRegion : s_Interior_Carriage_Return_New_Line_V2,
                    inputRegion: new Region() { StartLine = 3, StartColumn = 5, EndLine = 3, EndColumn = 7 }),

                new TestCaseData(outputRegion : s_Complete_File_V2,
                    inputRegion: new Region() { CharOffset = 0, CharLength = 20 }),

                new TestCaseData(outputRegion : s_Complete_File_V2,
                    inputRegion: new Region() { StartLine = 1, EndLine = 4, EndColumn = 4, CharOffset = 0 }),

                new TestCaseData(outputRegion : s_Complete_File_V2,
                    inputRegion: new Region() { StartLine = 1, EndLine = 4, EndColumn = 4, CharOffset = 0, CharLength = 20 }),

                new TestCaseData(outputRegion : s_Complete_File_V1,
                    inputRegion: new Region() { StartLine = 1, EndLine = 5, CharOffset = 0 }),

                new TestCaseData(outputRegion: s_Line_3,
                    inputRegion: new Region() {CharOffset = 11, CharLength = 4 }),

                new TestCaseData(outputRegion: s_Line_3,
                    inputRegion: new Region() {StartLine = 3 }),

                new TestCaseData(outputRegion: s_Lines_2_And_3,
                    inputRegion: new Region() {StartLine = 2, EndLine = 3 }),

                new TestCaseData(outputRegion: s_Interior_Characters,
                    inputRegion: new Region() {CharOffset = 12, CharLength = 2 })
    });

        private static readonly ReadOnlyCollection<TestCaseData> s_newLineTestCases =
            new ReadOnlyCollection<TestCaseData>(new TestCaseData[]
            { 
                // 
                // Sanity check sample with new line characters only
                new TestCaseData(outputRegion: s_Complete_File_New_Lines_Only,
                    inputRegion: new Region() { CharOffset = 0, CharLength = 12 }),

                new TestCaseData(outputRegion: s_Complete_File_New_Lines_Only,
                    inputRegion: new Region() { StartLine = 1, EndLine = 4, CharOffset = 0 }),

                new TestCaseData(outputRegion: s_Complete_File_New_Lines_Only,
                    inputRegion: new Region() { StartLine = 1, EndLine = 4, CharOffset = 0, CharLength = 12 }),


                new TestCaseData(outputRegion: s_Fragment_New_Lines_Only,
                    inputRegion: new Region() { CharOffset = 3, CharLength = 9 }),

                new TestCaseData(outputRegion: s_Fragment_New_Lines_Only,
                    inputRegion: new Region() { StartLine = 1, EndLine = 4, StartColumn = 4, EndColumn = 1 }),

                new TestCaseData(outputRegion: s_Fragment_New_Lines_Only,
                    inputRegion: new Region() { StartLine = 1, EndLine = 4, StartColumn = 4, EndColumn = 1, CharOffset = 0, CharLength = 9 })
            });

        private static readonly ReadOnlyCollection<TestCaseData> s_carriageReturnTestCasess =
            new ReadOnlyCollection<TestCaseData>(new TestCaseData[]
            {
                // 
                // Sanity check sample with carriage return characters only
                new TestCaseData(outputRegion: s_Complete_File_Carriage_Returns_Only,
                    inputRegion: new Region() { CharOffset = 0, CharLength = 10  }),

                new TestCaseData(outputRegion: s_Complete_File_Carriage_Returns_Only,
                    inputRegion: new Region() { StartLine = 1, EndLine = 6, EndColumn = 1, CharOffset = 0 }),

                new TestCaseData(outputRegion: s_Complete_File_Carriage_Returns_Only,
                    inputRegion: new Region() { StartLine = 1, EndLine = 6, EndColumn = 1, CharOffset = 0, CharLength = 10 }),

                new TestCaseData(outputRegion: s_Fragment_Carriage_Returns_Only,
                    inputRegion: new Region() { CharOffset = 4, CharLength = 6 }),

                new TestCaseData(outputRegion: s_Fragment_Carriage_Returns_Only,
                    inputRegion: new Region() { StartLine = 4, EndLine = 6, StartColumn = 2, EndColumn = 1 })
            });

        [Fact]
        public void FileRegionsCache_PopulatesFromMissingFile()
        {
            var run = new Run();
            var fileRegionsCache = new FileRegionsCache(run);

            Uri uri = new Uri(@"c:\temp\DoesNotExist\" + Guid.NewGuid().ToString() + ".cpp");

            Region region = new Region() { CharOffset = 17 };

            // Region should not be touched in any way if the file it references is missing
            fileRegionsCache.PopulateTextRegionProperties(region, uri, populateSnippet: false).ValueEquals(region).Should().BeTrue();
            fileRegionsCache.PopulateTextRegionProperties(region, uri, populateSnippet: true).ValueEquals(region).Should().BeTrue();
        }

        [Fact]
        public void FileRegionsCache_PopulatesSpecExampleRegions()
        {
            ExecuteTests(SPEC_EXAMPLE, s_specExampleTestCases);
        }

        [Fact]
        public void FileRegionsCache_PopulatesNewLineFileRegions()
        {
            ExecuteTests(COMPLETE_FILE_NEW_LINES_ONLY, s_newLineTestCases);
        }

        [Fact]
        public void FileRegionsCache_PopulatesCarriageReturnFileRegions()
        {
            ExecuteTests(COMPLETE_FILE_CARRIAGE_RETURNS_ONLY, s_carriageReturnTestCasess);
        }

        private static void ExecuteTests(string fileText, ReadOnlyCollection<TestCaseData> testCases)
        {
            Uri uri = new Uri(@"c:\temp\myFile.cpp");

            var run = new Run();
            IFileSystem mockFileSystem = MockFactory.MakeMockFileSystem(uri.LocalPath, fileText);
            var fileRegionsCache = new FileRegionsCache(run, fileSystem: mockFileSystem);

            ExecuteTests(testCases, fileRegionsCache, uri);
        }

        private static void ExecuteTests(ReadOnlyCollection<TestCaseData> testCases, FileRegionsCache fileRegionsCache, Uri uri)
        {
            foreach (TestCaseData testCase in testCases)
            {
                Region inputRegion = testCase.InputRegion;
                Region expectedRegion = testCase.OutputRegion.DeepClone();
                ArtifactContent snippet = expectedRegion.Snippet;

                expectedRegion.Snippet = null;
                Region actualRegion = fileRegionsCache.PopulateTextRegionProperties(inputRegion, uri, populateSnippet: false);

                actualRegion.ValueEquals(expectedRegion).Should().BeTrue();
                actualRegion.Snippet.Should().BeNull();

                expectedRegion.Snippet = snippet;
                actualRegion = fileRegionsCache.PopulateTextRegionProperties(inputRegion, uri, populateSnippet: true);

                actualRegion.ValueEquals(expectedRegion).Should().BeTrue();

                if (snippet == null)
                {
                    actualRegion.Snippet.Should().BeNull();
                }
                else
                {
                    actualRegion.Snippet.Text.Should().Be(snippet.Text);
                }
            }
        }

        /// <remarks>
        /// The test is skipped because the CI builds were intermittently
        /// failing due to the differences in runtime environment.
        /// https://github.com/microsoft/sarif-sdk/issues/1827
        /// </remarks>
        [Fact(Skip = "Flaky test. Results vary depending on environment.")]
        public void FileRegionsCache_ProperlyCaches()
        {
            Uri uri = new Uri(@"C:\Code\Program.cs");

            StringBuilder fileContents = new StringBuilder();
            for (int i = 0; i < 1000; ++i)
            {
                fileContents.AppendLine("0123456789");
            }

            Run run = new Run();
            IFileSystem mockFileSystem = MockFactory.MakeMockFileSystem(uri.LocalPath, fileContents.ToString());
            FileRegionsCache fileRegionsCache = new FileRegionsCache(run, fileSystem: mockFileSystem);

            Region region = new Region()
            {
                StartLine = 2,
                StartColumn = 1,
                EndLine = 3,
                EndColumn = 10,
            };

            Stopwatch w = Stopwatch.StartNew();

            for (int i = 0; i < 1000; ++i)
            {
                Region copy = region.DeepClone();
                Region populated = fileRegionsCache.PopulateTextRegionProperties(copy, uri, populateSnippet: true);
            }

            const int timeThreshold = 150;

            // Runtime should be way under 100ms if caching, and much longer otherwise
            w.Stop();
            Assert.True(w.ElapsedMilliseconds < timeThreshold, $"Time observed was {w.ElapsedMilliseconds}. Expected < {timeThreshold} ms.");
        }

        [Fact]
        public void FileRegionsCache_PopulatesNullRegion()
        {
            Uri uri = new Uri(@"c:\temp\myFile.cpp");

            var run = new Run();
            IFileSystem mockFileSystem = MockFactory.MakeMockFileSystem(uri.LocalPath, SPEC_EXAMPLE);
            var fileRegionsCache = new FileRegionsCache(run, fileSystem: mockFileSystem);

            Region region = fileRegionsCache.PopulateTextRegionProperties(inputRegion: null, uri: uri, populateSnippet: false);
            region.Should().BeNull();

            region = fileRegionsCache.PopulateTextRegionProperties(inputRegion: null, uri: uri, populateSnippet: true);
            region.Should().BeNull();
        }
    }
}
