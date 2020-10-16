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
        private Run GenerateRunForTest(Dictionary<string, ArtifactLocation> originalUriBaseIds)
        {
            return new Run
            {
                Artifacts = new List<Artifact>(new[]
                {
                    new Artifact { Location=new ArtifactLocation{ Uri=new Uri("src/file1.cs", UriKind.Relative), UriBaseId="%TEST1%", Index = 0 } },
                    new Artifact { Location=new ArtifactLocation{ Uri=new Uri("src/file2.dll", UriKind.Relative), UriBaseId="%TEST2%", Index = 1 } },
                    new Artifact { Location=new ArtifactLocation{ Uri=new Uri("src/archive.zip", UriKind.Relative), UriBaseId="%TEST1%", Index = 2 } },
                    new Artifact { Location=new ArtifactLocation{ Uri=new Uri("file3.cs", UriKind.Relative), Index = 3 }, ParentIndex = 2 },
                    new Artifact { Location=new ArtifactLocation{ Uri=new Uri("archive2.gz", UriKind.Relative), Index = 4 }, ParentIndex = 2 },
                    new Artifact { Location=new ArtifactLocation{ Uri=new Uri("file4.cs", UriKind.Relative), Index = 5 }, ParentIndex = 4 },
                }),

                OriginalUriBaseIds = originalUriBaseIds
            };
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_SetsAbsoluteURI()
        {
            var run = new Run
            {
                OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
                {
                    ["%TEST%"] = new ArtifactLocation { Uri = new Uri("C:/github/sarif/") }
                }
            };

            // Initializes visitor with run in order to retrieve uri base id mappings
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
            visitor.VisitRun(run);

            PhysicalLocation location = new PhysicalLocation() { ArtifactLocation = new ArtifactLocation { UriBaseId = "%TEST%", Uri = new Uri("src/file.cs", UriKind.Relative) } };

            PhysicalLocation newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.ArtifactLocation.UriBaseId.Should().BeNull();
            newLocation.ArtifactLocation.Uri.Should().BeEquivalentTo(new Uri("C:/github/sarif/src/file.cs"));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_DoesNotSetUriIfNotInDictionary()
        {
            var run = new Run
            {
                OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
                {
                    ["%TEST%"] = new ArtifactLocation { Uri = new Uri("C:/github/sarif/") }
                }
            };

            // Initializes visitor with run in order to retrieve uri base id mappings
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
            visitor.VisitRun(run);

            PhysicalLocation location = new PhysicalLocation() { ArtifactLocation = new ArtifactLocation { UriBaseId = "%TEST2%", Uri = new Uri("src/file.cs", UriKind.Relative) } };

            PhysicalLocation newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.ArtifactLocation.UriBaseId.Should().NotBeNull();
            newLocation.ArtifactLocation.Uri.Should().BeEquivalentTo(new Uri("src/file.cs", UriKind.Relative));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_DoesNotSetUriIfBaseIsNotSet()
        {
            var run = new Run
            {
                OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
                {
                    ["%TEST%"] = new ArtifactLocation { Uri = new Uri("C:/github/sarif/") }
                }
            };

            // Initializes visitor with run in order to retrieve uri base id mappings
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
            visitor.VisitRun(run);

            PhysicalLocation location = new PhysicalLocation() { ArtifactLocation = new ArtifactLocation { UriBaseId = null, Uri = new Uri("src/file.cs", UriKind.Relative) } };

            PhysicalLocation newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.ArtifactLocation.UriBaseId.Should().BeNull();
            newLocation.ArtifactLocation.Uri.Should().BeEquivalentTo(new Uri("src/file.cs", UriKind.Relative));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_SetsAbsoluteUriForAllApplicableFiles()
        {
            Run run = GenerateRunForTest(new Dictionary<string, ArtifactLocation>()
            {
                ["%TEST1%"] = new ArtifactLocation { Uri = new Uri(@"C:\srcroot\") },
                ["%TEST2%"] = new ArtifactLocation { Uri = new Uri(@"D:\bld\out\") }
            });

            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();

            Run newRun = visitor.VisitRun(run);

            // Validate.
            newRun.Artifacts[0].Location.Uri.ToString().Should().Be(@"file:///C:/srcroot/src/file1.cs");
            newRun.Artifacts[1].Location.Uri.ToString().Should().Be(@"file:///D:/bld/out/src/file2.dll");
            newRun.Artifacts[2].Location.Uri.ToString().Should().Be(@"file:///C:/srcroot/src/archive.zip");
            newRun.Artifacts[3].Location.Uri.ToString().Should().Be(@"file3.cs");
            newRun.Artifacts[4].Location.Uri.ToString().Should().Be(@"archive2.gz");
            newRun.Artifacts[5].Location.Uri.ToString().Should().Be(@"file4.cs");

            // Operation should zap all uri base ids
            newRun.Artifacts.Where(f => f.Location.UriBaseId != null).Any().Should().BeFalse();
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_DoesNotSetAbsoluteUriIfNotApplicable()
        {
            Dictionary<string, ArtifactLocation> uriMapping = new Dictionary<string, ArtifactLocation>()
            {
                ["%TEST3%"] = new ArtifactLocation { Uri = new Uri(@"C:\srcroot\") },
                ["%TEST4%"] = new ArtifactLocation { Uri = new Uri(@"D:\bld\out\") }
            };

            Run expectedRun = GenerateRunForTest(uriMapping);
            Run actualRun = expectedRun.DeepClone();

            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
            Run newRun = visitor.VisitRun(actualRun);

            expectedRun.ValueEquals(actualRun).Should().BeTrue();
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitSarifLog_MultipleRunsWithDifferentProperties_RebasesProperly()
        {
            Run runA = GenerateRunForTest(new Dictionary<string, ArtifactLocation>()
            {
                ["%TEST1%"] = new ArtifactLocation { Uri = new Uri(@"C:\srcroot\") },
                ["%TEST2%"] = new ArtifactLocation { Uri = new Uri(@"D:\bld\out\") }
            });
            Run runB = GenerateRunForTest(new Dictionary<string, ArtifactLocation>()
            {
                ["%TEST1%"] = new ArtifactLocation { Uri = new Uri(@"C:\src\abc\") },
                ["%TEST2%"] = new ArtifactLocation { Uri = new Uri(@"D:\bld\123\") }
            });
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();

            SarifLog log = new SarifLog() { Runs = new Run[] { runA, runB } };
            SarifLog newLog = visitor.VisitSarifLog(log);

            // Validate
            newLog.Runs.Should().HaveCount(2);
            newLog.Runs[0].Artifacts.Should().NotIntersectWith(newLog.Runs[1].Artifacts);
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

            // Pass relative URI where absolute expected.
            Action action = () =>
            {
                MakeUrisAbsoluteVisitor.CombineUris(
                    absoluteBaseUri: relativeUri,
                    relativeUri: relativeUri);
            };

            action.Should().Throw<ArgumentException>();

            // Pass absolute URI where relative expected.
            action = () =>
            {
                MakeUrisAbsoluteVisitor.CombineUris(
                    absoluteBaseUri: absoluteUri,
                    relativeUri: absoluteUri);
            };

            action.Should().Throw<ArgumentException>();
        }
    }
}
