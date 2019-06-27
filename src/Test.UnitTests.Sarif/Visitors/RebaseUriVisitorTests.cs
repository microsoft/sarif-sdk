// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class RebaseUriVisitorTests
    {
        private readonly ITestOutputHelper output;

        public RebaseUriVisitorTests(ITestOutputHelper testOutput)
        {
            output = testOutput;
        }

        [Theory]
        [InlineData("BLDROOT", @"C:\blddir\out\test.dll", @"C:\blddir\out\", "test.dll")]
        [InlineData("SRCROOT", @"C:\blddir\out\test.dll", @"C:\blddir\src\", null)]
        public void RebaseUriVisitor_VisitPhysicalLocation_RebasesUri_WhenAppropriate(string rootName, string locationUriStr, string baseUriStr, string expectedDifference)
        {
            Uri locationUri = new Uri(locationUriStr);
            Uri baseUri = new Uri(baseUriStr);
            PhysicalLocation location = new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Uri = locationUri
                }
            };
            RebaseUriVisitor visitor = new RebaseUriVisitor(rootName, baseUri);
            PhysicalLocation newLocation = visitor.VisitPhysicalLocation(location);

            if (!string.IsNullOrEmpty(expectedDifference))
            {
                newLocation.ArtifactLocation.UriBaseId.Should().BeEquivalentTo(rootName, because: "we should set the root name for these.");
                newLocation.ArtifactLocation.Uri.Should().BeEquivalentTo(baseUri.MakeRelativeUri(locationUri), because: "the base URI should be relative if the expected difference is there.");
                newLocation.ArtifactLocation.Uri.ToString().Should().BeEquivalentTo(expectedDifference);
            }
            else
            {
                newLocation.Should().BeEquivalentTo(location, "When we have no expected difference, we expect the location to not be changed by the rebase operation.");
            }
        }

        [Fact]
        public void RebaseUriVisitor_VisitPhysicalLocation_DoesNotRebaseAlreadyRebasedUri()
        {
            PhysicalLocation location = new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Uri = new Uri(@"C:\bld\src\test.dll"),
                    UriBaseId = "BLDROOT"
                }
            };
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\bld\src\"));

            rebaseUriVisitor.VisitPhysicalLocation(location).Should().BeEquivalentTo(location, because: "we should not rebase a URI multiple times.");
        }

        [Fact]
        public void RebaseUriVisitor_VisitPhysicalLocation_DoesNothingIfIndexReferenceToRunArtifacts()
        {
            PhysicalLocation location = new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Index = 23
                }
            };
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\bld\src\"));

            rebaseUriVisitor.VisitPhysicalLocation(location).Should().BeEquivalentTo(location, because: "artifact location does not need to be rebased.");
        }

        [Fact]
        public void RebaseUriVisitor_VisitRun_AddsBaseUriDictionaryWhenNotPresent()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);

            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\src\root"));

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            IDictionary<string, ArtifactLocation> baseUriDictionary = newRun.OriginalUriBaseIds;

            baseUriDictionary.Should().ContainKey("SRCROOT");
            baseUriDictionary["SRCROOT"].ValueEquals(new ArtifactLocation { Uri = new Uri(@"C:\src\root") }).Should().BeTrue();
        }

        [Fact]
        public void RebaseUriVisitor_VisitRun_UpdatesBaseUriDictionaryWhenPresent()
        {
            const string srcRoot = "SRCROOT";
            Uri srcRootUri = new Uri(@"C:\src\root");

            const string bldRoot = "BLDROOT";
            Uri bldRootUri = new Uri(@"C:\bld\root");

            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor(srcRoot, srcRootUri);

            var oldDictionary = new Dictionary<string, ArtifactLocation>() { { bldRoot, new ArtifactLocation { Uri = bldRootUri } } };
            oldRun.OriginalUriBaseIds = oldDictionary;

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            IDictionary<string, ArtifactLocation> baseUriDictionary = newRun.OriginalUriBaseIds;

            baseUriDictionary.Should().ContainKey(srcRoot);
            baseUriDictionary[srcRoot].Uri.Should().BeEquivalentTo(srcRootUri);
            baseUriDictionary.Should().ContainKey(bldRoot);
            baseUriDictionary[bldRoot].Uri.Should().BeEquivalentTo(bldRootUri);
        }

        [Fact]
        public void RebaseUriVisitor_VisitRun_CorrectlyPatchesFileDictionaryKeys()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);

            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\src\"));

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            newRun.OriginalUriBaseIds.Should().ContainKey("SRCROOT");

            newRun.Artifacts.Where(f => f.Location.Uri.OriginalString.StartsWith(@"C:\src\")).Should().BeEmpty();
        }

        [Fact]
        public void RebaseUriVisitor_VisitRun_DoesNotPatchFileDictionaryKeysWhenNotABaseUri()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);

            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\bld\"));

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            newRun.OriginalUriBaseIds.Should().ContainKey("SRCROOT");

            // Random sarif log generator uses "C:\src\" as the root.
            newRun.Artifacts.Should().BeEquivalentTo(oldRun.Artifacts);
        }

        [Fact]
        public void RebaseUriVisitor_VisitFileData_PatchesParentUri()
        {
            Uri rootfileUri = new Uri(@"file://C:/src/root/blah.zip");
            Uri childFileUri = new Uri(@"/stuff.doc", UriKind.RelativeOrAbsolute);

            Artifact rootFileData = new Artifact() { Location = new ArtifactLocation { Uri = rootfileUri }, ParentIndex = -1 };
            Artifact childFileData = new Artifact() { Location = new ArtifactLocation { Uri = childFileUri }, ParentIndex = 0 };
            Run run = new Run
            {
                Artifacts = new List<Artifact>
                {
                    new Artifact { Location = new ArtifactLocation { Uri = rootfileUri, Index = 0 }, ParentIndex = -1 },
                    new Artifact { Location = new ArtifactLocation { Uri = childFileUri, Index = 1 }, ParentIndex = 0 },
                    new Artifact { Location = new ArtifactLocation { Uri = childFileUri, Index = 2 }, ParentIndex = -1 }
                },
                Results = new List<Result> { new Result { Locations = new List<Location> { new Location { PhysicalLocation = new PhysicalLocation() { ArtifactLocation = new ArtifactLocation() { Uri = childFileUri, Index = 1 } } } } } }
            };

            string srcroot = "SRCROOT";
            Uri rootUriBaseId = new Uri(@"C:\src\root\");
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor(srcroot, rootUriBaseId);

            run = rebaseUriVisitor.VisitRun(run);

            run.Artifacts[0].Location.Uri.Should().Be("blah.zip");
            run.Artifacts[0].Location.UriBaseId.Should().Be("SRCROOT");
            run.OriginalUriBaseIds.Should().ContainKey(srcroot);
            run.OriginalUriBaseIds[srcroot].Uri.Should().Be(@"C:\src\root\");
        }

        [Fact]
        public void RebaseUriVisitor_VisitFileData_RebasesAllTheThings()
        {
            string comprehensiveSarifPath = Path.Combine(Environment.CurrentDirectory, @"v2\SpecExamples\Comprehensive.sarif");

            string inputText = File.ReadAllText(comprehensiveSarifPath);

            SarifLog sarifLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(inputText, formatting: Formatting.None, out inputText);

            sarifLog.Runs.Count().Should().Be(1);

            var visitor = new RebaseVerifyingVisitor();
            visitor.VisitRun(sarifLog.Runs[0]);

            string outputText = JsonConvert.SerializeObject(sarifLog, Formatting.Indented);

            string uriRootText = "file:///home/buildAgent/";
            string toolsRootBaseId = "TOOLS_ROOT";
            string srcRootBaseId = "SRCROOT";

            int uriCount = 19;
            int toolsRootUriBaseIdCount = 4;
            int srcRootUriBaseIdCount = 1;
            int uriBaseIdCount = toolsRootUriBaseIdCount + srcRootUriBaseIdCount;
            int uriRootTextCount = 13;

            visitor.FileLocationUriBaseIds.Count.Should().Be(uriCount);
            visitor.FileLocationUriBaseIds.Where(u => u == null).Count().Should().Be(uriCount - uriBaseIdCount);
            visitor.FileLocationUriBaseIds.Where(u => u != null).Count().Should().Be(uriBaseIdCount);
            visitor.FileLocationUriBaseIds.Where(u => u == toolsRootBaseId).Count().Should().Be(toolsRootUriBaseIdCount);
            visitor.FileLocationUriBaseIds.Where(u => u == srcRootBaseId).Count().Should().Be(srcRootUriBaseIdCount);

            visitor.FileLocationUris.Count.Should().Be(uriCount);
            visitor.FileLocationUris.Where(u => u != null && u.StartsWith(uriRootText)).Count().Should().Be(uriRootTextCount);

            string agentRootBaseId = "AGENT_ROOT";

            var rebaseUriVisitor = new RebaseUriVisitor(agentRootBaseId, new Uri(uriRootText));
            Run rebasedRun = rebaseUriVisitor.VisitRun(sarifLog.Runs[0]);

            outputText = JsonConvert.SerializeObject(sarifLog, Formatting.Indented);

            visitor = new RebaseVerifyingVisitor();
            visitor.VisitRun(rebasedRun);

            visitor.FileLocationUriBaseIds.Count.Should().Be(uriCount);
            visitor.FileLocationUriBaseIds.Where(u => u == null).Count().Should().Be(1);
            visitor.FileLocationUriBaseIds.Where(u => u == toolsRootBaseId).Count().Should().Be(toolsRootUriBaseIdCount);
            visitor.FileLocationUriBaseIds.Where(u => u == srcRootBaseId).Count().Should().Be(srcRootUriBaseIdCount);
            visitor.FileLocationUriBaseIds.Where(u => u == agentRootBaseId).Count().Should().Be(uriRootTextCount);

            visitor.FileLocationUris.Count.Should().Be(uriCount);

            // The AGENT_ROOT originalUriBaseId should _not_ be counted as a file location.
            visitor.FileLocationUris.Where(u => u != null && u.StartsWith(uriRootText)).Count().Should().Be(0);
        }

        private class RebaseVerifyingVisitor : SarifRewritingVisitor
        {
            public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
            {
                if (_currentRun.OriginalUriBaseIds == null || !_currentRun.OriginalUriBaseIds.Values.Contains(node))
                {

                    FileLocationUris = FileLocationUris ?? new List<string>();
                    FileLocationUris.Add(node.Uri.OriginalString);

                    FileLocationUriBaseIds = FileLocationUriBaseIds ?? new List<string>();
                    FileLocationUriBaseIds.Add(node.UriBaseId);
                }
                return base.VisitArtifactLocation(node);
            }

            private Run _currentRun;

            public override Run VisitRun(Run node)
            {
                _currentRun = node;
                return base.VisitRun(node);
            }

            public List<string> FileLocationUris { get; set; }
            public List<string> FileLocationUriBaseIds { get; set; }
        }
    }
}
