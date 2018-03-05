using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class RebaseUriVisitorTests
    {
        [Theory]
        [InlineData("BLDROOT", @"C:\blddir\out\test.dll", @"C:\blddir\out\", "test.dll")]
        [InlineData("SRCROOT", @"C:\blddir\out\test.dll", @"C:\blddir\src\", null)]
        public void RebaseUriVisitor_VisitPhysicalLocation_RebasesUri_WhenAppropriate(string rootName, string locationUriStr, string baseUriStr, string expectedDifference)
        {
            Uri locationUri = new Uri(locationUriStr);
            Uri baseUri = new Uri(baseUriStr);
            PhysicalLocation location = new PhysicalLocation(locationUri, null, null);
            RebaseUriVisitor visitor = new RebaseUriVisitor(rootName, baseUri);
            PhysicalLocation newLocation = visitor.VisitPhysicalLocation(location);

            if (!string.IsNullOrEmpty(expectedDifference))
            {
                newLocation.UriBaseId.ShouldBeEquivalentTo(rootName, "We should set the root name for these.");
                newLocation.Uri.ShouldBeEquivalentTo(baseUri.MakeRelativeUri(locationUri), "Base URI should be relative if the expected difference is there.");
                newLocation.Uri.ToString().ShouldBeEquivalentTo(expectedDifference, "We expect this difference.");
            } else
            {
                newLocation.ShouldBeEquivalentTo(location, "When we have no expected difference, we expect the location to not be changed by the rebase operation.");
            }
        }

        [Fact]
        public void RebaseUriVisitor_VisitPhysicalLocation_DoesNotRebaseAlreadyRebasedUri()
        {
            PhysicalLocation location = new PhysicalLocation(new Uri(@"C:\bld\src\test.dll"), "BLDROOT", null);
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\bld\src\"));

            rebaseUriVisitor.VisitPhysicalLocation(location).ShouldBeEquivalentTo(location, "We should not rebase a URI multiple times..");
        }
        
        [Fact]
        public void RebaseUriVisitor_VisitRun_AddsBaseUriDictionaryWhenNotPresent()
        {
            // Slightly roundabout.  We want to randomly test this, but we also want to be able to repeat this if the test fails.
            int randomSeed = (new Random()).Next();
            Random random = new Random(randomSeed);

            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);

            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\src\root"));

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            newRun.Properties.Should().ContainKey(RebaseUriVisitor.BaseUriDictionaryName);

            Dictionary<string, Uri> baseUriDictionary = RebaseUriVisitor.DeserializePropertyDictionary(newRun.Properties[RebaseUriVisitor.BaseUriDictionaryName]);

            baseUriDictionary.Should().ContainKey("SRCROOT");
            baseUriDictionary.Should().ContainValue(new Uri(@"C:\src\root"));
        }

        [Fact]
        public void RebaseUriVisitor_VisitRun_UpdatesBaseUriDictionaryWhenPresent()
        {
            Uri srcRootUri = new Uri(@"C:\src\root");
            const string srcRoot = "SRCROOT";

            const string bldRoot = "BLDROOT";
            Uri bldRootUri = new Uri(@"C:\bld\root");

            // Slightly roundabout.  We want to randomly test this, but we also want to be able to repeat this if the test fails.
            int randomSeed = (new Random()).Next();
            Random random = new Random(randomSeed);
            
            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);
            oldRun.Properties = new Dictionary<string, SerializedPropertyInfo>();
            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor(srcRoot, srcRootUri);

            Dictionary<string, Uri> oldDictionary = new Dictionary<string, Uri>() { { bldRoot, bldRootUri } };

            oldRun.Properties.Add(RebaseUriVisitor.BaseUriDictionaryName, RebaseUriVisitor.ReserializePropertyDictionary(oldDictionary));

            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            newRun.Properties.Should().ContainKey(RebaseUriVisitor.BaseUriDictionaryName);

            Dictionary<string, Uri> baseUriDictionary = RebaseUriVisitor.DeserializePropertyDictionary(newRun.Properties[RebaseUriVisitor.BaseUriDictionaryName]);

            baseUriDictionary.Should().ContainKey(srcRoot);
            baseUriDictionary[srcRoot].ShouldBeEquivalentTo(srcRootUri);
            baseUriDictionary.Should().ContainKey(bldRoot);
            baseUriDictionary[bldRoot].ShouldBeEquivalentTo(bldRootUri);
        }

        [Fact]
        public void RebaseUriVisitor_VisitRun_ReplacesBaseUriDictionaryWhenIncorrect()
        {
            // Slightly roundabout.  We want to randomly test this, but we also want to be able to repeat this if the test fails.
            int randomSeed = (new Random()).Next();
            Random random = new Random(randomSeed);

            Run oldRun = RandomSarifLogGenerator.GenerateRandomRun(random);
            oldRun.Properties = new Dictionary<string, SerializedPropertyInfo>();
            SerializedPropertyInfo oldData = new SerializedPropertyInfo("42", false);
            oldRun.Properties.Add(RebaseUriVisitor.BaseUriDictionaryName, oldData);

            RebaseUriVisitor rebaseUriVisitor = new RebaseUriVisitor("SRCROOT", new Uri(@"C:\src\root"));
            
            Run newRun = rebaseUriVisitor.VisitRun(oldRun);

            newRun.Properties.Should().ContainKey(RebaseUriVisitor.BaseUriDictionaryName);

            Dictionary<string, Uri> baseUriDictionary = RebaseUriVisitor.DeserializePropertyDictionary(newRun.Properties[RebaseUriVisitor.BaseUriDictionaryName]);

            baseUriDictionary.Should().ContainKey("SRCROOT");
            baseUriDictionary["SRCROOT"].ShouldBeEquivalentTo(new Uri(@"C:\src\root"));

            baseUriDictionary.Should().ContainKey(RebaseUriVisitor.BaseUriDictionaryName + RebaseUriVisitor.IncorrectlyFormattedDictionarySuffix);
            baseUriDictionary[RebaseUriVisitor.BaseUriDictionaryName + RebaseUriVisitor.IncorrectlyFormattedDictionarySuffix].ShouldBeEquivalentTo(oldData);
        }


    }
}
