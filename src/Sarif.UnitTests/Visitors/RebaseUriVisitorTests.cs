// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
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
                newLocation.FileLocation.UriBaseId.Should().BeEquivalentTo(rootName, "We should set the root name for these.");
                newLocation.FileLocation.Uri.Should().BeEquivalentTo(baseUri.MakeRelativeUri(locationUri), "Base URI should be relative if the expected difference is there.");
                newLocation.FileLocation.Uri.ToString().Should().BeEquivalentTo(expectedDifference, "We expect this difference.");
            } else
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

            rebaseUriVisitor.VisitPhysicalLocation(location).Should().BeEquivalentTo(location, "We should not rebase a URI multiple times.");
        }
        
        [Fact]
        public void RebaseUriVisitor_VisitRun_AddsBaseUriDictionaryWhenNotPresent()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);

            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\src\root"));

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            IDictionary<string, Uri> baseUriDictionary = newRun.OriginalUriBaseIds;

            baseUriDictionary.Should().ContainKey("SRCROOT");
            baseUriDictionary.Should().ContainValue(new Uri(@"C:\src\root"));
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

            Dictionary<string, Uri> oldDictionary = new Dictionary<string, Uri>() { { bldRoot, bldRootUri } };
            oldRun.OriginalUriBaseIds = oldDictionary;

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            IDictionary<string, Uri> baseUriDictionary = newRun.OriginalUriBaseIds;

            baseUriDictionary.Should().ContainKey(srcRoot);
            baseUriDictionary[srcRoot].Should().BeEquivalentTo(srcRootUri);
            baseUriDictionary.Should().ContainKey(bldRoot);
            baseUriDictionary[bldRoot].Should().BeEquivalentTo(bldRootUri);
        }

        [Fact]
        public void RebaseUriVisitor_VisitRun_CorrectlyPatchesFileDictionaryKeys()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);
            
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\src\"));

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            newRun.OriginalUriBaseIds.Should().ContainKey("SRCROOT");
            
            newRun.Files.Keys.Where(k => k.StartsWith(@"C:\src\")).Should().BeEmpty();
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
            newRun.Files.Keys.Should().BeEquivalentTo(oldRun.Files.Keys);
         }

        [Fact]
        public void RebaseUriVisitor_VisitFileData_PatchesUriAndParentUri()
        {
            Uri fileUri = new Uri(@"file://C:/src/root/blah.zip#/stuff.doc");
            string parentKey = @"C:\src\root\blah.zip";
            FileData fileData = new FileData() { FileLocation = new FileLocation { Uri = fileUri }, ParentKey = parentKey};
            Run run = new Run() { Files = new Dictionary<string, FileData>() { { fileUri.ToString(), fileData } } };

            string srcroot = "SRCROOT";
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor(srcroot, new Uri(@"C:\src\root\"));

            rebaseUriVisitor.FixFiles(run);

            run.Files.Should().ContainKey("blah.zip#/stuff.doc");
            var newFileData = run.Files["blah.zip#/stuff.doc"];

            newFileData.FileLocation.Uri.IsAbsoluteUri.Should().BeFalse();
            newFileData.FileLocation.Uri.Should().NotBeSameAs(fileUri);
            newFileData.FileLocation.UriBaseId.Should().Be(srcroot);
            newFileData.ParentKey.Should().NotBeSameAs(parentKey);
        }

        [Fact]
        public void RebaseUriVisitor_VisitFileData_DoesNotPatchUriAndParentWhenNotAppropriate()
        {
            Uri fileUri = new Uri(@"file://C:/src/root/blah.zip#/stuff.doc");
            string parentKey = @"C:\src\root\blah.zip";
            FileData fileData = new FileData() { FileLocation = new FileLocation { Uri = fileUri }, ParentKey = parentKey };
            Run run = new Run() { Files = new Dictionary<string, FileData>() { { fileUri.ToString(), fileData } } };

            string bldroot = "BLDROOT";
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor(bldroot, new Uri(@"C:\bld\"));
            
            rebaseUriVisitor.FixFiles(run);

            run.Files.Should().ContainKey(fileUri.ToString());
            var newFileData = run.Files[fileUri.ToString()];

            newFileData.FileLocation.Uri.Should().BeSameAs(fileUri);
            newFileData.FileLocation.UriBaseId.Should().BeNullOrEmpty();
            newFileData.ParentKey.Should().BeSameAs(parentKey);
        }
    }
}
