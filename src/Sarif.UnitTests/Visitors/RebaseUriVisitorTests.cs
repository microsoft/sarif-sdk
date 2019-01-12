﻿// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
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
                FileLocation = new FileLocation
                {
                    Uri = locationUri
                }
            };
            RebaseUriVisitor visitor = new RebaseUriVisitor(rootName, baseUri);
            PhysicalLocation newLocation = visitor.VisitPhysicalLocation(location);

            if (!string.IsNullOrEmpty(expectedDifference))
            {
                newLocation.FileLocation.UriBaseId.Should().BeEquivalentTo(rootName, because: "we should set the root name for these.");
                newLocation.FileLocation.Uri.Should().BeEquivalentTo(baseUri.MakeRelativeUri(locationUri), because: "the base URI should be relative if the expected difference is there.");
                newLocation.FileLocation.Uri.ToString().Should().BeEquivalentTo(expectedDifference);
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
                FileLocation = new FileLocation
                {
                    Uri = new Uri(@"C:\bld\src\test.dll"),
                    UriBaseId = "BLDROOT"
                }
            };
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\bld\src\"));

            rebaseUriVisitor.VisitPhysicalLocation(location).Should().BeEquivalentTo(location, because: "we should not rebase a URI multiple times.");
        }

        [Fact]
        public void RebaseUriVisitor_VisitRun_AddsBaseUriDictionaryWhenNotPresent()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);

            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\src\root"));

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            IDictionary<string, FileLocation> baseUriDictionary = newRun.OriginalUriBaseIds;

            baseUriDictionary.Should().ContainKey("SRCROOT");
            baseUriDictionary["SRCROOT"].ValueEquals(new FileLocation { Uri = new Uri(@"C:\src\root") }).Should().BeTrue();
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

            var oldDictionary = new Dictionary<string, FileLocation>() { { bldRoot, new FileLocation { Uri = bldRootUri } } };
            oldRun.OriginalUriBaseIds = oldDictionary;

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            IDictionary<string, FileLocation> baseUriDictionary = newRun.OriginalUriBaseIds;

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

            newRun.Files.Where(f => f.FileLocation.Uri.OriginalString.StartsWith(@"C:\src\")).Should().BeEmpty();
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
            newRun.Files.Should().BeEquivalentTo(oldRun.Files);
        }

        [Fact]
        public void RebaseUriVisitor_VisitFileData_PatchesParentUri()
        {
            Uri rootfileUri = new Uri(@"file://C:/src/root/blah.zip#/stuff.doc");
            Uri childFileUri = new Uri(@"/stuff.doc");

            FileData rootFileData = new FileData() { FileLocation = new FileLocation { Uri = rootfileUri }, ParentIndex = -1 };
            FileData childFileData = new FileData() { FileLocation = new FileLocation { Uri = childFileUri }, ParentIndex = 0 };
            Run run = new Run
            {
                Files = new List<FileData>
                {
                    new FileData { FileLocation = new FileLocation { Uri = rootfileUri, FileIndex = -0 }, ParentIndex = -1 },
                    new FileData { FileLocation = new FileLocation { Uri = childFileUri, FileIndex = 1 }, ParentIndex = 0 },
                    new FileData { FileLocation = new FileLocation { Uri = childFileUri, FileIndex = -1 }, ParentIndex = -1 }
                },
                Results = new List<Result> { new Result { Locations = new List<Location> { new Location { PhysicalLocation = new PhysicalLocation() { FileLocation = new FileLocation() { Uri = childFileUri, FileIndex = 1 } } } } } }
            };

            string srcroot = "SRCROOT";
            Uri rootUriBaseId = new Uri(@"C:\src\root\");
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor(srcroot, rootUriBaseId);

            run = rebaseUriVisitor.VisitRun(run);

            run.Files[0].FileLocation.Uri.Should().Be(rootUriBaseId);            
            run.OriginalUriBaseIds.Should().ContainKey(srcroot);
            run.OriginalUriBaseIds[srcroot].Uri.Should().Be(@"C:\src\root\");
        }

        [Fact]
        public void RebaseUriVisitor_VisitFileData_RebasesAllTheThings()
        {
            string comprehensiveSarifPath = Path.Combine(Environment.CurrentDirectory, @"v2\SpecExamples\Comprehensive.sarif");

            string sarifText = File.ReadAllText(comprehensiveSarifPath);

            SarifLog sarifLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(sarifText, forceUpdate: false, formatting: Formatting.None, out sarifText);

            sarifLog.Runs.Count().Should().Be(1);

            var visitor = new RebaseVerifyingVisitor();
            visitor.VisitRun(sarifLog.Runs[0]);

            string uriRootText = "file:///home/buildAgent/";
            string toolsRootBaseId = "TOOLS_ROOT";

            visitor.FileDataParentKeys.Count.Should().Be(4);
            visitor.FileDataParentKeys.Where(k => k == null).Count().Should().Be(3);
            visitor.FileDataParentKeys.Where(k => k != null && k.StartsWith(uriRootText)).Count().Should().Be(1);

            visitor.FileDataKeys.Count.Should().Be(4);
            visitor.FileDataKeys.Where(k => k != null && k.StartsWith(uriRootText)).Count().Should().Be(3);
            visitor.FileDataKeys.Where(k => k != null && k.StartsWith("#" + toolsRootBaseId + "#")).Count().Should().Be(1);

            int uriCount = 17;

            visitor.FileLocationUriBaseIds.Count.Should().Be(uriCount);
            visitor.FileLocationUriBaseIds.Where(u => u == null).Count().Should().Be(13);
            visitor.FileLocationUriBaseIds.Where(u => u == toolsRootBaseId).Count().Should().Be(3);

            visitor.FileLocationUris.Count.Should().Be(uriCount);
            visitor.FileLocationUris.Where(u => u != null && u.StartsWith(uriRootText)).Count().Should().Be(11);

            string agentRootBaseId = "AGENT_ROOT";

            var rebaseUriVisitor = new RebaseUriVisitor(agentRootBaseId, new Uri(uriRootText));
            Run rebasedRun = rebaseUriVisitor.VisitRun(sarifLog.Runs[0]);

            visitor = new RebaseVerifyingVisitor();
            visitor.VisitRun(rebasedRun);

            visitor.FileDataKeys.Count.Should().Be(4);
            visitor.FileDataKeys.Where(k => k != null && k.StartsWith("#" + toolsRootBaseId + "#")).Count().Should().Be(1);
            visitor.FileDataKeys.Where(k => k != null && k.StartsWith("#" + agentRootBaseId + "#")).Count().Should().Be(3);

            visitor.FileDataParentKeys.Count.Should().Be(4);
            visitor.FileDataParentKeys.Where(k => k == null).Count().Should().Be(3);
            visitor.FileDataParentKeys.Where(k => k != null && k.StartsWith("#" + agentRootBaseId + "#")).Count().Should().Be(1);

            // Rebasing AGENT_ROOT adds a new file location to uriBaseIds
            visitor.FileLocationUriBaseIds.Count.Should().Be(uriCount + 1);
            visitor.FileLocationUriBaseIds.Where(u => u == null).Count().Should().Be(3);
            visitor.FileLocationUriBaseIds.Where(u => u == toolsRootBaseId).Count().Should().Be(3);
            visitor.FileLocationUriBaseIds.Where(u => u == agentRootBaseId).Count().Should().Be(11);

            visitor.FileLocationUris.Count.Should().Be(uriCount + 1);

            // The AGENT_ROOT originalUriBaseId is the last thing that will include the uriRootText value
            visitor.FileLocationUris.Where(u => u != null && u.StartsWith(uriRootText)).Count().Should().Be(1);
        }

        private class RebaseVerifyingVisitor : SarifRewritingVisitor
        {
            public RebaseVerifyingVisitor()
            {
            }

            public override FileLocation VisitFileLocation(FileLocation node)
            {
                FileLocationUris = FileLocationUris ?? new List<string>();
                FileLocationUris.Add(node.Uri.OriginalString);

                FileLocationUriBaseIds = FileLocationUriBaseIds ?? new List<string>();
                FileLocationUriBaseIds.Add(node.UriBaseId);

                return base.VisitFileLocation(node);
            }

            public List<string> FileDataKeys { get; set; }
            public List<string> FileDataParentKeys { get; set; }
            public List<string> FileLocationUris { get; set; }
            public List<string> FileLocationUriBaseIds { get; set; }
        }
    }
}
