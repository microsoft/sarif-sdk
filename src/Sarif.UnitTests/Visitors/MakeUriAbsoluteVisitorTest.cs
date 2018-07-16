// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class MakeUriAbsoluteVisitorTest
    {
        private Run GenerateRunForTest(Dictionary<string, Uri> uriBaseIdMapping)
        {
            Run run = new Run();
            run.Files = new Dictionary<string, FileData>()
            {
                { "src/file1.cs", new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/file1.cs", UriKind.Relative),UriBaseId="%TEST1%" }, ParentKey=null } },
                { "src/file2.dll", new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/file2.dll", UriKind.Relative),UriBaseId="%TEST2%" }, ParentKey=null } },
                { "src/archive.zip", new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip", UriKind.Relative),UriBaseId="%TEST1%" }, ParentKey=null } },
                { "src/archive.zip#file3.cs", new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip#file3.cs", UriKind.Relative),UriBaseId="%TEST1%" }, ParentKey="src/archive.zip" } },
                { "src/archive.zip#archive2.gz", new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip#archive2.gz", UriKind.Relative),UriBaseId="%TEST1%" }, ParentKey="src/archive.zip" } },
                { "src/archive.zip#archive2.gz/file4.cs", new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip#archive2.gz/file4.cs", UriKind.Relative),UriBaseId="%TEST1%" }, ParentKey="src/archive.zip#archive2.gz" } },
                { "src/archive.zip#file5.cs", new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip#file5.cs", UriKind.Relative),UriBaseId="%TEST1%" }, ParentKey="src/archive.zip" } },
            };

            if (uriBaseIdMapping != null)
            {
                run.Properties = new Dictionary<string, SerializedPropertyInfo>();

                run.Properties[RebaseUriVisitor.BaseUriDictionaryName] = RebaseUriVisitor.ReserializePropertyDictionary(uriBaseIdMapping);
            }
            return run;
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_SetsAbsoluteURI()
        {
            PhysicalLocation location = new PhysicalLocation() { FileLocation = new FileLocation { UriBaseId = "%TEST%", Uri = new Uri("src/file.cs", UriKind.Relative) } };
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();
            visitor._currentUriMappings = new Dictionary<string, Uri>() { { "%TEST%", new Uri("C:/github/sarif/") } };
            var newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.FileLocation.UriBaseId.Should().BeNull();
            newLocation.FileLocation.Uri.Should().BeEquivalentTo(new Uri("C:/github/sarif/src/file.cs"));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_DoesNotSetUriIfNotInDictionary()
        {

            PhysicalLocation location = new PhysicalLocation() { FileLocation = new FileLocation { UriBaseId = "%TEST2%", Uri = new Uri("src/file.cs", UriKind.Relative) } };
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();
            visitor._currentUriMappings = new Dictionary<string, Uri>() { { "%TEST%", new Uri("C:/github/sarif/") } };
            var newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.FileLocation.UriBaseId.Should().NotBeNull();
            newLocation.FileLocation.Uri.Should().BeEquivalentTo(new Uri("src/file.cs", UriKind.Relative));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitPhysicalLocation_DoesNotSetUriIfBaseIsNotSet()
        {
            PhysicalLocation location = new PhysicalLocation() { FileLocation = new FileLocation { UriBaseId = null, Uri = new Uri("src/file.cs", UriKind.Relative) } };
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();
            visitor._currentUriMappings = new Dictionary<string, Uri>() { { "%TEST%", new Uri("C:/github/sarif/") } };
            var newLocation = visitor.VisitPhysicalLocation(location);
            newLocation.FileLocation.UriBaseId.Should().BeNull();
            newLocation.FileLocation.Uri.Should().BeEquivalentTo(new Uri("src/file.cs", UriKind.Relative));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_SetsAbsoluteUriForAllApplicableFiles()
        {
            Run run = GenerateRunForTest(new Dictionary<string, Uri>()
            {
                { "%TEST1%", new Uri(@"C:\srcroot\") },
                { "%TEST2%", new Uri(@"D:\bld\out\") }
            });
            
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();

            var newRun = visitor.VisitRun(run);
            // Validate.
            newRun.Files.ContainsKey(@"file://C:/srcroot/src/file1.cs");
            newRun.Files.ContainsKey(@"file://D:/bld/out/src/file2.dll");
            newRun.Files.ContainsKey(@"file://C:/srcroot/src/archive.zip");
            newRun.Files.ContainsKey(@"file://C:/srcroot/src/archive.zip#file3.cs");

            foreach (var key in newRun.Files.Keys)
            {
                newRun.Files[key].FileLocation.Uri.Should().BeEquivalentTo(new Uri(key));
                newRun.Files[key].FileLocation.UriBaseId.Should().BeNull();
                if (!string.IsNullOrEmpty(newRun.Files[key].ParentKey))
                {
                    newRun.Files.Should().ContainKey(newRun.Files[key].ParentKey);
                }
            }
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_DoesNotSetAbsoluteUriIfNotApplicable()
        {
            Dictionary<string, Uri> uriMapping = new Dictionary<string, Uri>()
            {
                { "%TEST3%", new Uri(@"C:\srcroot\") },
                { "%TEST4%", new Uri(@"D:\bld\out\") },
            };

            Run run = GenerateRunForTest(uriMapping);

            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();

            var newRun = visitor.VisitRun(run);

            var oldRun = GenerateRunForTest(uriMapping);
            // Validate.

            newRun.Files.Keys.Should().BeEquivalentTo(oldRun.Files.Keys);
            foreach(var key in newRun.Files.Keys)
            {
                oldRun.Files[key].FileLocation.Uri.Should().BeEquivalentTo(newRun.Files[key].FileLocation.Uri);
                oldRun.Files[key].FileLocation.UriBaseId.Should().BeEquivalentTo(newRun.Files[key].FileLocation.UriBaseId);
                oldRun.Files[key].ParentKey.Should().BeEquivalentTo(newRun.Files[key].ParentKey);
            }
        }


        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_DoesNotChangeIfPropertiesAbsent()
        {
            Run run = GenerateRunForTest(null);

            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();

            var newRun = visitor.VisitRun(run);

            // Validate.
            var oldRun = GenerateRunForTest(null);

            newRun.Files.Keys.Should().BeEquivalentTo(oldRun.Files.Keys);
            foreach (var key in newRun.Files.Keys)
            {
                oldRun.Files[key].FileLocation.Uri.Should().BeEquivalentTo(newRun.Files[key].FileLocation.Uri);
                oldRun.Files[key].FileLocation.UriBaseId.Should().BeEquivalentTo(newRun.Files[key].FileLocation.UriBaseId);
                oldRun.Files[key].ParentKey.Should().BeEquivalentTo(newRun.Files[key].ParentKey);
            }
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_ThrowsIfPropertiesAreWrong()
        {
            Run run = GenerateRunForTest(null);
            run.Properties = new Dictionary<string, SerializedPropertyInfo>();
            run.Properties[RebaseUriVisitor.BaseUriDictionaryName] = new SerializedPropertyInfo("\"this is a string\"", true);
            
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();
            Assert.Throws<InvalidOperationException>(()=> visitor.VisitRun(run));
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitSarifLog_MultipleRunsWithDifferentProperties_RebasesProperly()
        {
            Run runA = GenerateRunForTest(new Dictionary<string, Uri>()
            {
                { "%TEST1%", new Uri(@"C:\srcroot\") },
                { "%TEST2%", new Uri(@"D:\bld\out\") },
            });
            Run runB = GenerateRunForTest(new Dictionary<string, Uri>()
            {
                { "%TEST1%", new Uri(@"C:\src\abc") },
                { "%TEST2%", new Uri(@"D:\bld\123\") },
            });
            AbsoluteUrisVisitor visitor = new AbsoluteUrisVisitor();

            SarifLog log = new SarifLog() { Runs=new Run[] { runA, runB } };
            SarifLog newLog = visitor.VisitSarifLog(log);

            // Validate
            newLog.Runs.Should().HaveCount(2);
            newLog.Runs[0].Files.Keys.Should().NotIntersectWith(newLog.Runs[1].Files.Keys);
        }
    }
}
