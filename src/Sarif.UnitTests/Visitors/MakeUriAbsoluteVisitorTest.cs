// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
            run.Files = new List<FileData>(new[]
            {
                new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/file1.cs", UriKind.Relative), UriBaseId="%TEST1%", FileIndex = 0 } },
                new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/file2.dll", UriKind.Relative), UriBaseId="%TEST2%", FileIndex = 1 } },
                new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip", UriKind.Relative), UriBaseId="%TEST1%", FileIndex = 2 } },
                new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip#file3.cs", UriKind.Relative), UriBaseId="%TEST1%", FileIndex = 3 } },
                new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip#archive2.gz", UriKind.Relative), UriBaseId="%TEST1%", FileIndex = 4 } },
                new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip#archive2.gz/file4.cs", UriKind.Relative), UriBaseId="%TEST1%", FileIndex = 5 }, },
                new FileData() { FileLocation=new FileLocation{ Uri=new Uri("src/archive.zip#file5.cs", UriKind.Relative), UriBaseId="%TEST1%", FileIndex = 6 },  }
            });

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
            Run run = GenerateRunForTest(new Dictionary<string, Uri>()
            {
                { "%TEST1%", new Uri(@"C:\srcroot\") },
                { "%TEST2%", new Uri(@"D:\bld\out\") }
            });
            
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();

            var newRun = visitor.VisitRun(run);
            // Validate.
            newRun.Files[0].FileLocation.Uri.ToString().Should().Be(@"file://C:/srcroot/src/file1.cs");
            newRun.Files[1].FileLocation.Uri.ToString().Should().Be(@"file://D:/bld/out/src/file2.dll");
            newRun.Files[2].FileLocation.Uri.ToString().Should().Be(@"file://C:/srcroot/src/archive.zip");
            newRun.Files[3].FileLocation.Uri.ToString().Should().Be(@"file://C:/srcroot/src/archive.zip#file3.cs");
            newRun.Files[4].FileLocation.Uri.ToString().Should().Be(@"file://C:/srcroot/src/archive.zip#archive2.gz");
            newRun.Files[5].FileLocation.Uri.ToString().Should().Be(@"file://C:/srcroot/src/archive.zip#archive2.gz/file4.cs");
            newRun.Files[6].FileLocation.Uri.ToString().Should().Be(@"file://C:/srcroot/src/archive.zip#file5.cs");

            // Operation should zap all uri base ids
            newRun.Files.Select(f => f.FileLocation.UriBaseId != null).Any().Should().BeFalse();
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_DoesNotSetAbsoluteUriIfNotApplicable()
        {
            Dictionary<string, Uri> uriMapping = new Dictionary<string, Uri>()
            {
                { "%TEST3%", new Uri(@"C:\srcroot\") },
                { "%TEST4%", new Uri(@"D:\bld\out\") },
            };

            Run expectedRun = GenerateRunForTest(uriMapping);
            Run actualRun = expectedRun.DeepClone();


            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
            var newRun = visitor.VisitRun(actualRun);

            expectedRun.Should().Be(actualRun);
        }

        [Fact]
        public void MakeUriAbsoluteVisitor_VisitRun_ThrowsIfPropertiesAreWrong()
        {
            Run run = GenerateRunForTest(null);
            run.Properties = new Dictionary<string, SerializedPropertyInfo>();
            run.Properties[RebaseUriVisitor.BaseUriDictionaryName] = new SerializedPropertyInfo("\"this is a string\"", true);
            
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
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
            MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();

            SarifLog log = new SarifLog() { Runs=new Run[] { runA, runB } };
            SarifLog newLog = visitor.VisitSarifLog(log);

            // Validate
            newLog.Runs.Should().HaveCount(2);
            newLog.Runs[0].Files.Should().NotIntersectWith(newLog.Runs[1].Files);
        }
    }
}
