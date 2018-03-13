// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class MakeUriAbsoluteVisitorTest
    {
        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_SetsAbsoluteURI()
        {
            PhysicalLocation location = new PhysicalLocation() {UriBaseId="%TEST%", Uri=new Uri("src/file.cs", UriKind.Relative), };
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();
            visitor.uriMappings = new Dictionary<string, Uri>() { { "%TEST%", new Uri("C:/github/sarif/") } };
            var newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.UriBaseId.Should().BeNull();
            newLocation.Uri.ShouldBeEquivalentTo(new Uri("C:/github/sarif/src/file.cs"));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_DoesNotSetUriIfNotInDictionary()
        {

            PhysicalLocation location = new PhysicalLocation() { UriBaseId = "%TEST2%", Uri = new Uri("src/file.cs", UriKind.Relative), };
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();
            visitor.uriMappings = new Dictionary<string, Uri>() { { "%TEST%", new Uri("C:/github/sarif/") } };
            var newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.UriBaseId.Should().NotBeNull();
            newLocation.Uri.ShouldBeEquivalentTo(new Uri("src/file.cs", UriKind.Relative));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_DoesNotSetUriIfBaseIsNotSet()
        {
            PhysicalLocation location = new PhysicalLocation() { UriBaseId = null, Uri = new Uri("src/file.cs", UriKind.Relative), };
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();
            visitor.uriMappings = new Dictionary<string, Uri>() { { "%TEST%", new Uri("C:/github/sarif/") } };
            var newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.UriBaseId.Should().BeNull();
            newLocation.Uri.ShouldBeEquivalentTo(new Uri("src/file.cs", UriKind.Relative));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_SetsAbsoluteUriForAllApplicableFiles()
        {
            Run run = new Run();
            run.Files = new Dictionary<string, FileData>()
            {
                {"src/file1.cs", new FileData(){Uri=new Uri("src/file1.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/file2.dll", new FileData(){Uri=new Uri("src/file2.cs", UriKind.Relative), UriBaseId="%TEST2%",ParentKey="" } },
                {"src/archive.zip", new FileData(){Uri=new Uri("src/archive.zip", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/archive.zip#file3.cs", new FileData(){Uri=new Uri("src/archive.zip#file3.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz/file4.cs", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz/file4.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip#archive2.gz" } },
                {"src/archive.zip#file5.cs", new FileData(){Uri=new Uri("src/archive.zip#file5.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
            };
            run.Properties = new Dictionary<string, SerializedPropertyInfo>();
            Dictionary<string, Uri> uriBaseIdMapping = new Dictionary<string, Uri>()
            {
                { "%TEST1%", new Uri(@"C:\srcroot\") },
                { "%TEST2%", new Uri(@"D:\bld\out\") },
            };
            run.Properties[RebaseUriVisitor.BaseUriDictionaryName] = RebaseUriVisitor.ReserializePropertyDictionary(uriBaseIdMapping);

        
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();

            var newRun = visitor.VisitRun(run);
            // Validate. TODO
            run.Files.ContainsKey("");


        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_DoesNotSetAbsoluteUriIfNotApplicable()
        {
            Run run = new Run();
            run.Files = new Dictionary<string, FileData>()
            {
                {"src/file1.cs", new FileData(){Uri=new Uri("src/file1.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/file2.dll", new FileData(){Uri=new Uri("src/file2.cs", UriKind.Relative), UriBaseId="%TEST2%",ParentKey="" } },
                {"src/archive.zip", new FileData(){Uri=new Uri("src/archive.zip", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/archive.zip#file3.cs", new FileData(){Uri=new Uri("src/archive.zip#file3.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz/file4.cs", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz/file4.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip#archive2.gz" } },
                {"src/archive.zip#file5.cs", new FileData(){Uri=new Uri("src/archive.zip#file5.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
            };
            run.Properties = new Dictionary<string, SerializedPropertyInfo>();
            Dictionary<string, Uri> uriBaseIdMapping = new Dictionary<string, Uri>()
            {
                { "%TEST1%", new Uri(@"C:\srcroot\") },
                { "%TEST2%", new Uri(@"D:\bld\out\") },
            };
            run.Properties[RebaseUriVisitor.BaseUriDictionaryName] = RebaseUriVisitor.ReserializePropertyDictionary(uriBaseIdMapping);


            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();

            var newRun = visitor.VisitRun(run);
            // Validate. TODO
            run.Files.Should().ContainKey("");
        }


        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_DoesNotChangeIfPropertiesAbsent()
        {
            Run run = new Run();
            run.Files = new Dictionary<string, FileData>()
            {
                {"src/file1.cs", new FileData(){Uri=new Uri("src/file1.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/file2.dll", new FileData(){Uri=new Uri("src/file2.cs", UriKind.Relative), UriBaseId="%TEST2%",ParentKey="" } },
                {"src/archive.zip", new FileData(){Uri=new Uri("src/archive.zip", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/archive.zip#file3.cs", new FileData(){Uri=new Uri("src/archive.zip#file3.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz/file4.cs", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz/file4.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip#archive2.gz" } },
                {"src/archive.zip#file5.cs", new FileData(){Uri=new Uri("src/archive.zip#file5.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
            };
            run.Properties = new Dictionary<string, SerializedPropertyInfo>();
            Dictionary<string, Uri> uriBaseIdMapping = new Dictionary<string, Uri>()
            {
                { "%TEST1%", new Uri(@"C:\srcroot\") },
                { "%TEST2%", new Uri(@"D:\bld\out\") },
            };
            run.Properties[RebaseUriVisitor.BaseUriDictionaryName] = RebaseUriVisitor.ReserializePropertyDictionary(uriBaseIdMapping);


            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();

            var newRun = visitor.VisitRun(run);
            // Validate.  TODO
            run.Files.Should().ContainKey("");
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_ThrowsIfPropertiesAreWrong()
        {
            Run run = new Run();
            run.Files = new Dictionary<string, FileData>()
            {
                {"src/file1.cs", new FileData(){Uri=new Uri("src/file1.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/file2.dll", new FileData(){Uri=new Uri("src/file2.cs", UriKind.Relative), UriBaseId="%TEST2%",ParentKey="" } },
                {"src/archive.zip", new FileData(){Uri=new Uri("src/archive.zip", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/archive.zip#file3.cs", new FileData(){Uri=new Uri("src/archive.zip#file3.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz/file4.cs", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz/file4.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip#archive2.gz" } },
                {"src/archive.zip#file5.cs", new FileData(){Uri=new Uri("src/archive.zip#file5.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
            };
            run.Properties = new Dictionary<string, SerializedPropertyInfo>();
            run.Properties[RebaseUriVisitor.BaseUriDictionaryName] = new SerializedPropertyInfo("\"this is a string\"", true);


            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();
            //todo--should throw
            visitor.VisitRun(run);
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitSarifLog_MultipleRunsWithDifferentProperties_RebasesProperly()
        {
            Run runA = new Run();
            runA.Files = new Dictionary<string, FileData>()
            {
                {"src/file1.cs", new FileData(){Uri=new Uri("src/file1.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/file2.dll", new FileData(){Uri=new Uri("src/file2.cs", UriKind.Relative), UriBaseId="%TEST2%",ParentKey="" } },
                {"src/archive.zip", new FileData(){Uri=new Uri("src/archive.zip", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/archive.zip#file3.cs", new FileData(){Uri=new Uri("src/archive.zip#file3.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz/file4.cs", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz/file4.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip#archive2.gz" } },
                {"src/archive.zip#file5.cs", new FileData(){Uri=new Uri("src/archive.zip#file5.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
            };
            runA.Properties = new Dictionary<string, SerializedPropertyInfo>();
            Dictionary<string, Uri> uriBaseIdMapping = new Dictionary<string, Uri>()
            {
                { "%TEST1%", new Uri(@"C:\srcroot\") },
                { "%TEST2%", new Uri(@"D:\bld\out\") },
            };
            runA.Properties[RebaseUriVisitor.BaseUriDictionaryName] = RebaseUriVisitor.ReserializePropertyDictionary(uriBaseIdMapping);
            Run runB = new Run();
            runA.Files = new Dictionary<string, FileData>()
            {
                {"src/file1.cs", new FileData(){Uri=new Uri("src/file1.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/file2.dll", new FileData(){Uri=new Uri("src/file2.cs", UriKind.Relative), UriBaseId="%TEST2%",ParentKey="" } },
                {"src/archive.zip", new FileData(){Uri=new Uri("src/archive.zip", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="" } },
                {"src/archive.zip#file3.cs", new FileData(){Uri=new Uri("src/archive.zip#file3.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
                {"src/archive.zip#archive2.gz/file4.cs", new FileData(){Uri=new Uri("src/archive.zip#archive2.gz/file4.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip#archive2.gz" } },
                {"src/archive.zip#file5.cs", new FileData(){Uri=new Uri("src/archive.zip#file5.cs", UriKind.Relative), UriBaseId="%TEST1%",ParentKey="src/archive.zip" } },
            };
            runB.Properties = new Dictionary<string, SerializedPropertyInfo>();
            Dictionary<string, Uri> uriBaseIdMappingB = new Dictionary<string, Uri>()
            {
                { "%TEST1%", new Uri(@"C:\src\abc") },
                { "%TEST2%", new Uri(@"D:\bld\123\") },
            };
            runB.Properties[RebaseUriVisitor.BaseUriDictionaryName] = RebaseUriVisitor.ReserializePropertyDictionary(uriBaseIdMapping);

            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();

            SarifLog log = new SarifLog() { Runs=new Run[] { runA, runB } };
            // Validate. TODO
            runA.Files.ContainsKey("");
            runB.Files.ContainsKey("");
        }
    }
}
