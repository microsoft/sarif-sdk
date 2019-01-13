// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class MakeUriAbsoluteVisitorTest
    {
        private Run GenerateRunForTest(Dictionary<string, FileLocation> originalUriBaseIds)
        {
            return new Run
            {
                Files = new List<FileData>(new[]
                {
                    new FileData { FileLocation=new FileLocation{ Uri=new Uri("src/file1.cs", UriKind.Relative), UriBaseId="%TEST1%", FileIndex = 0 } },
                    new FileData { FileLocation=new FileLocation{ Uri=new Uri("src/file2.dll", UriKind.Relative), UriBaseId="%TEST2%", FileIndex = 1 } },
                    new FileData { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip", UriKind.Relative), UriBaseId="%TEST1%", FileIndex = 2 } },
                    new FileData { FileLocation=new FileLocation{ Uri=new Uri("file3.cs", UriKind.Relative), FileIndex = 3 }, ParentIndex = 2 },
                    new FileData { FileLocation=new FileLocation{ Uri=new Uri("archive2.gz", UriKind.Relative), FileIndex = 4 }, ParentIndex = 2 },
                    new FileData { FileLocation=new FileLocation{ Uri=new Uri("file4.cs", UriKind.Relative), FileIndex = 5 }, ParentIndex = 4 },
                }),

                OriginalUriBaseIds = originalUriBaseIds
            };
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_SetsAbsoluteURI()
        {
            var run = new Run
            {
                OriginalUriBaseIds = new Dictionary<string, FileLocation>
                {
                    ["%TEST%"] = new FileLocation { Uri = new Uri("C:/github/sarif/") }
                }
            };

            // Initializes visitor with run in order to retrieve uri base id mappings
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
            visitor.VisitRun(run);

            PhysicalLocation location = new PhysicalLocation() { FileLocation = new FileLocation { UriBaseId = "%TEST%", Uri = new Uri("src/file.cs", UriKind.Relative) } };

            var newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.FileLocation.UriBaseId.Should().BeNull();
            newLocation.FileLocation.Uri.Should().BeEquivalentTo(new Uri("C:/github/sarif/src/file.cs"));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_DoesNotSetUriIfNotInDictionary()
        {
            var run = new Run
            {
                OriginalUriBaseIds = new Dictionary<string, FileLocation>
                {
                    ["%TEST%"] = new FileLocation { Uri = new Uri("C:/github/sarif/") }
                }
            };

            // Initializes visitor with run in order to retrieve uri base id mappings
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
            visitor.VisitRun(run);

            PhysicalLocation location = new PhysicalLocation() { FileLocation = new FileLocation { UriBaseId = "%TEST2%", Uri = new Uri("src/file.cs", UriKind.Relative) } };

            var newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.FileLocation.UriBaseId.Should().NotBeNull();
            newLocation.FileLocation.Uri.Should().BeEquivalentTo(new Uri("src/file.cs", UriKind.Relative));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_DoesNotSetUriIfBaseIsNotSet()
        {
            var run = new Run
            {
                OriginalUriBaseIds = new Dictionary<string, FileLocation>
                {
                    ["%TEST%"] = new FileLocation { Uri = new Uri("C:/github/sarif/") }
                }
            };

            // Initializes visitor with run in order to retrieve uri base id mappings
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
            visitor.VisitRun(run);

            PhysicalLocation location = new PhysicalLocation() { FileLocation = new FileLocation { UriBaseId = null, Uri = new Uri("src/file.cs", UriKind.Relative) } };

            var newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.FileLocation.UriBaseId.Should().BeNull();
            newLocation.FileLocation.Uri.Should().BeEquivalentTo(new Uri("src/file.cs", UriKind.Relative));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_SetsAbsoluteUriForAllApplicableFiles()
        {
            Run run = GenerateRunForTest(new Dictionary<string, FileLocation>()
            {
                ["%TEST1%"] = new FileLocation { Uri = new Uri(@"C:\srcroot\") },
                ["%TEST2%"] = new FileLocation { Uri = new Uri(@"D:\bld\out\") }
            });

            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();

            var newRun = visitor.VisitRun(run);

            // Validate.
            newRun.Files[0].FileLocation.Uri.ToString().Should().Be(@"file:///C:/srcroot/src/file1.cs");
            newRun.Files[1].FileLocation.Uri.ToString().Should().Be(@"file:///D:/bld/out/src/file2.dll");
            newRun.Files[2].FileLocation.Uri.ToString().Should().Be(@"file:///C:/srcroot/src/archive.zip");
            newRun.Files[3].FileLocation.Uri.ToString().Should().Be(@"file3.cs");
            newRun.Files[4].FileLocation.Uri.ToString().Should().Be(@"archive2.gz");
            newRun.Files[5].FileLocation.Uri.ToString().Should().Be(@"file4.cs");

            // Operation should zap all uri base ids
            newRun.Files.Where(f => f.FileLocation.UriBaseId != null).Any().Should().BeFalse();
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_DoesNotSetAbsoluteUriIfNotApplicable()
        {
            Dictionary<string, FileLocation> uriMapping = new Dictionary<string, FileLocation>()
            {
                ["%TEST3%"] = new FileLocation { Uri = new Uri(@"C:\srcroot\") },
                ["%TEST4%"] = new FileLocation { Uri = new Uri(@"D:\bld\out\") }
            };

            Run expectedRun = GenerateRunForTest(uriMapping);
            Run actualRun = expectedRun.DeepClone();

            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
            var newRun = visitor.VisitRun(actualRun);

            expectedRun.ValueEquals(actualRun).Should().BeTrue();
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitSarifLog_MultipleRunsWithDifferentProperties_RebasesProperly()
        {
            Run runA = GenerateRunForTest(new Dictionary<string, FileLocation>()
            {
                ["%TEST1%"] = new FileLocation { Uri = new Uri(@"C:\srcroot\") },
                ["%TEST2%"] = new FileLocation { Uri = new Uri(@"D:\bld\out\") }
            });
            Run runB = GenerateRunForTest(new Dictionary<string, FileLocation>()
            {
                ["%TEST1%"] = new FileLocation { Uri = new Uri(@"C:\src\abc\") },
                ["%TEST2%"] = new FileLocation { Uri = new Uri(@"D:\bld\123\") }
            });
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();

            SarifLog log = new SarifLog() { Runs = new Run[] { runA, runB } };
            SarifLog newLog = visitor.VisitSarifLog(log);

            // Validate
            newLog.Runs.Should().HaveCount(2);
            newLog.Runs[0].Files.Should().NotIntersectWith(newLog.Runs[1].Files);
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_CombineUriFunctionsProperly()
        {
            var testCases = new Tuple<string, string, string>[]
            {
                new Tuple<string, string, string>
                    (@"https://base/", @"relative/file.cpp",  "https://base/relative/file.cpp")
            };

            foreach (Tuple<string, string, string> testCase in testCases)
            {
                MakeUrisAbsoluteVisitor.CombineUris(
                    absoluteBaseUri: new Uri(testCase.Item1, UriKind.Absolute),
                    relativeUri: new Uri(testCase.Item2, UriKind.Relative))
                        .Should().Be(testCase.Item3);
            }
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_CombineUriValidatesArgumentsProperly()
        {
            Uri absoluteUri = new Uri("https://absolute.example.com", UriKind.Absolute);
            Uri relativeUri = new Uri("relative/someResource", UriKind.Relative);

            // First, ensure that our test data succeeds when used properly
            MakeUrisAbsoluteVisitor.CombineUris(
                absoluteBaseUri: absoluteUri,
                relativeUri: relativeUri);
            
            // Pass relative uri where absolute expected
            var action = new Action(() => 
                {
                    MakeUrisAbsoluteVisitor.CombineUris(
                        absoluteBaseUri: relativeUri,
                        relativeUri: relativeUri);
                });

            action.Should().Throw<ArgumentException>();

            // Pass absolute uri where relative expected
            action = new Action(() =>
            {
                MakeUrisAbsoluteVisitor.CombineUris(
                    absoluteBaseUri: absoluteUri,
                    relativeUri: absoluteUri);
            });

            action.Should().Throw<ArgumentException>();
        }
    }
}
