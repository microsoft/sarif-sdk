// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FxCopLogReaderTests
    {
        [Fact]
        public void FxCopLogReader_Read_NullContext_NullInput()
        {
            Assert.Throws<ArgumentNullException>(() => new FxCopLogReader().Read(null, null));
        }

        [Fact]
        public void FxCopLogReader_Read_NullInput()
        {
            var context = new FxCopLogReader.Context();
            Assert.Throws<ArgumentNullException>(() => new FxCopLogReader().Read(context, null));
        }

        [Fact]
        public void FxCopLogReader_Read_BadStartTag()
        {
            using (MemoryStream input = Utilities.CreateStreamFromString(FxCopTestData.FxCopReportBadStartTag))
            {
                var context = new FxCopLogReader.Context();
                Assert.Throws<XmlException>(() => new FxCopLogReader().Read(context, input));
            }
        }

        [Fact]
        public void FxCopLogReader_Read_BadXml()
        {
            using (MemoryStream input = Utilities.CreateStreamFromString(FxCopTestData.FxCopReportBadXml))
            {
                var context = new FxCopLogReader.Context();
                Assert.Throws<XmlException>(() => new FxCopLogReader().Read(context, input));
            }
        }
    }

    public class FxCopLogReader_ContextTests
    {
        [Fact]
        public void FxCopLogReader_Context_RefineReport()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineReport("myreport");
            Assert.Equal("myreport", context.Report);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineResource()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineResource("myresource.resx");
            Assert.Equal("myresource.resx", context.Resource);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineException()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineException(true, "CA0001", "mytarget");
            Assert.True(context.Exception);
            Assert.Equal("CA0001", context.CheckId);
            Assert.Equal("mytarget", context.ExceptionTarget);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineExceptionType()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineExceptionType("mytype");
            Assert.Equal("mytype", context.ExceptionType);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineExceptionMessage()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineExceptionMessage("mymessage");
            Assert.Equal("mymessage", context.ExceptionMessage);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineStackTrace()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineStackTrace(@"trace\n trace");
            context.StackTrace.Should().BeCrossPlatformEquivalentStrings(@"trace\n trace");
        }

        [Fact]
        public void FxCopLogReader_Context_RefineInnerExceptionType()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineInnerExceptionType(@"myinnertype");
            Assert.Equal(@"myinnertype", context.InnerExceptionType);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineInnerExceptionMessage()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineInnerExceptionMessage(@"myinnermessage");
            Assert.Equal(@"myinnermessage", context.InnerExceptionMessage);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineInnerStackTrace()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineInnerStackTrace(@"myinnertrace");
            Assert.Equal(@"myinnertrace", context.InnerStackTrace);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineTarget()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineTarget("mybinary.dll");
            Assert.Equal("mybinary.dll", context.Target);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineModule()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineModule("mybinary.dll");
            Assert.Equal("mybinary.dll", context.Module);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineNamespace()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineNamespace("mynamespace");
            Assert.Equal("mynamespace", context.Namespace);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineType()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineType("mytype");
            Assert.Equal("mytype", context.Type);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineMember()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineMember("mymember(string)");
            Assert.Equal("mymember(string)", context.Member);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineMessage()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineMessage("CA0000", "VeryUsefulCheck", "1", "MyCategory", "Breaking", "ExcludedInSource");
            Assert.Equal("CA0000", context.CheckId);
            Assert.Equal("1", context.MessageId);
            Assert.Equal("MyCategory", context.Category);
            Assert.Equal("VeryUsefulCheck", context.Typename);
            Assert.Equal("Breaking", context.FixCategory);
            Assert.Equal("ExcludedInSource", context.Status);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineIssue()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineIssue("hello!", "test", "25", "error", "source", "myfile.cs", 13);
            Assert.Equal("hello!", context.Message);
            Assert.Equal("test", context.ResolutionName);
            Assert.Equal("25", context.Certainty);
            Assert.Equal("error", context.Level);
            Assert.Equal("source", context.Path);
            Assert.Equal("myfile.cs", context.File);
            Assert.Equal(13, context.Line.Value);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineProjectToMemberIssue()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineTarget("mybinary.dll");
            context.RefineModule("mybinary.dll");
            context.RefineNamespace("mynamespace");
            context.RefineType("mytype");
            context.RefineMember("mymember(string)");
            context.RefineMessage("CA0000", "VeryUsefulCheck", "1", "MyCategory", "Breaking", "Excluded");
            context.RefineIssue("hello!", "test", "25", "error", "source", "myfile.cs", 13);

            Assert.Equal("mybinary.dll", context.Target);
            Assert.Equal("mybinary.dll", context.Module);
            Assert.Equal("mynamespace", context.Namespace);
            Assert.Equal("mytype", context.Type);
            Assert.Equal("mymember(string)", context.Member);
            Assert.Equal("CA0000", context.CheckId);
            Assert.Equal("1", context.MessageId);
            Assert.Equal("MyCategory", context.Category);
            Assert.Equal("VeryUsefulCheck", context.Typename);
            Assert.Equal("Breaking", context.FixCategory);
            Assert.Equal("hello!", context.Message);
            Assert.Equal("test", context.ResolutionName);
            Assert.Equal("25", context.Certainty);
            Assert.Equal("error", context.Level);
            Assert.Equal("source", context.Path);
            Assert.Equal("myfile.cs", context.File);
            Assert.Equal("Excluded", context.Status);
            Assert.Equal(13, context.Line.Value);

            context.ClearTarget();
            Assert.Null(context.Target);
            Assert.Null(context.Module);
            Assert.Null(context.Namespace);
            Assert.Null(context.Type);
            Assert.Null(context.Member);
            Assert.Null(context.Message);
            Assert.Null(context.ResolutionName);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineProjectToResourceIssue()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineTarget("mybinary.dll");
            context.RefineModule("mybinary.dll");
            context.RefineResource("myresource.resx");
            context.RefineMessage("CA0000", "VeryUsefulCheck", "1", "MyCategory", "Breaking", null);

            context.RefineIssue("hello!", "test", "25", "error", "source", "myresource.resx", 13);
            Assert.Equal("mybinary.dll", context.Target);
            Assert.Equal("mybinary.dll", context.Module);
            Assert.Equal("myresource.resx", context.Resource);
            Assert.Equal("CA0000", context.CheckId);
            Assert.Equal("1", context.MessageId);
            Assert.Equal("MyCategory", context.Category);
            Assert.Equal("VeryUsefulCheck", context.Typename);
            Assert.Equal("Breaking", context.FixCategory);
            Assert.Equal("hello!", context.Message);
            Assert.Equal("test", context.ResolutionName);
            Assert.Equal("25", context.Certainty);
            Assert.Equal("error", context.Level);
            Assert.Equal("source", context.Path);
            Assert.Null(context.Status);
            Assert.Equal("myresource.resx", context.File);
            Assert.Equal(13, context.Line.Value);

            context.ClearTarget();
            Assert.Null(context.Target);
            Assert.Null(context.Module);
            Assert.Null(context.Resource);
            Assert.Null(context.Namespace);
            Assert.Null(context.Type);
            Assert.Null(context.Member);
            Assert.Null(context.Message);
            Assert.Null(context.ResolutionName);
        }

        [Fact]
        public void FxCopLogReader_Context_RefineProjectToExceptionIssue()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineException(true, "CA0001", "binary.dll#namespace#member(string)");
            context.RefineExceptionType("type");
            context.RefineExceptionMessage("message");
            context.RefineStackTrace("trace");
            context.RefineInnerExceptionType("innertype");
            context.RefineInnerExceptionMessage("innermessage");
            context.RefineInnerStackTrace("innertrace");

            string exception = FxCopLogReader.MakeExceptionMessage("Rule", "CA1000", context.ExceptionType, context.ExceptionMessage, context.StackTrace, context.InnerExceptionType, context.InnerExceptionMessage, context.InnerStackTrace);
            context.RefineIssue(exception, null, null, null, null, null, null);

            Assert.Equal("Rule CA1000 exception: type: message trace. Inner Exception: innertype: innermessage innertrace", context.Message);
            Assert.Equal("binary.dll#namespace#member(string)", context.ExceptionTarget);

            context.ClearException();
            Assert.Null(context.Target);
            Assert.Null(context.Module);
            Assert.Null(context.Namespace);
            Assert.Null(context.Type);
            Assert.Null(context.Member);
            Assert.Null(context.Message);
            Assert.Null(context.ResolutionName);

            Assert.False(context.Exception);
            Assert.Null(context.ExceptionType);
            Assert.Null(context.ExceptionMessage);
            Assert.Null(context.StackTrace);
            Assert.Null(context.InnerExceptionType);
            Assert.Null(context.InnerExceptionMessage);
            Assert.Null(context.InnerStackTrace);
        }
    }

    public class FxCopConverterTests
    {
        private static void ValidateLogicalLocations(IList<LogicalLocation> expectedLogicalLocations, IList<LogicalLocation> actualLogicalLocations)
        {
            // If we end up with more shared helper code, we could extend these tests types from a common base.
            AndroidStudioConverterTests.ValidateLogicalLocations(expectedLogicalLocations, actualLogicalLocations);
        }

        [Fact]
        public void FxCopConverter_Convert_NullInput()
        {
            var converter = new FxCopConverter();
            Assert.Throws<ArgumentNullException>(() => converter.Convert(null, null, OptionallyEmittedData.None));
        }

        [Fact]
        public void FxCopConverter_Convert_NullOutput()
        {
            var converter = new FxCopConverter();
            Assert.Throws<ArgumentNullException>(() => converter.Convert(new MemoryStream(), null, OptionallyEmittedData.None));
        }

        [Fact]
        public void FxCopConverter_Convert_InvalidInput()
        {
            var converter = new FxCopConverter();
            Assert.Throws<XmlException>(() => Utilities.GetConverterJson(converter, FxCopTestData.FxCopReportInvalid));
        }

        [Fact]
        public void FxCopConverter_CreateResult_FakeContext_Member()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineTarget(@"mybinary.dll");
            context.RefineModule("mybinary.dll");
            context.RefineNamespace("mynamespace");
            context.RefineType("mytype");
            context.RefineMember("mymember(string)");
            context.RefineMessage("CA0000", "VeryUsefulCheck", "1", "FakeCategory", "Breaking", "ExcludedInSource");
            context.RefineIssue(null, "test", "uncertain", "error", @"source", "myfile.cs", 13);
            context.RefineItem("hello!");

            string expectedLogicalLocation = "mynamespace.mytype.mymember(string)";

            var expectedResult = new Result
            {
                RuleId = "CA0000",
                Message = new Message { Arguments = new List<string>(new string[] { "hello!" }) },
                Suppressions = new List<Suppression> { new Suppression { Kind = SuppressionKind.InSource } },
                PartialFingerprints = new Dictionary<string, string>(),
                AnalysisTarget = new ArtifactLocation
                {
                    Uri = new Uri("mybinary.dll", UriKind.RelativeOrAbsolute),
                },
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri("source\\myfile.cs", UriKind.RelativeOrAbsolute)
                            },
                            Region = new Region { StartLine = 13 }
                        },
                        LogicalLocation = new LogicalLocation
                        {
                            FullyQualifiedName = expectedLogicalLocation,
                            Index = 3
                        }
                    }
                }
            };

            expectedResult.PartialFingerprints.Add("UniqueId", "1#test");
            expectedResult.SetProperty("Level", "error");
            expectedResult.SetProperty("Category", "FakeCategory");
            expectedResult.SetProperty("FixCategory", "Breaking");

            var expectedLogicalLocations = new List<LogicalLocation>
            {
                new LogicalLocation { ParentIndex = -1, Name = "mybinary.dll", Kind = LogicalLocationKind.Module },
                new LogicalLocation { ParentIndex = 0, Name = "mynamespace", FullyQualifiedName = "mybinary.dll!mynamespace", Kind = LogicalLocationKind.Namespace },
                new LogicalLocation { ParentIndex = 1, Name = "mytype", FullyQualifiedName = "mybinary.dll!mynamespace.mytype", Kind = LogicalLocationKind.Type },
                new LogicalLocation { ParentIndex = 2, Name = "mymember(string)", FullyQualifiedName = "mybinary.dll!mynamespace.mytype.mymember(string)", Kind = LogicalLocationKind.Member }
            };
            var converter = new FxCopConverter();
            Result result = converter.CreateResult(context);

            ValidateLogicalLocations(expectedLogicalLocations, converter.LogicalLocations);
        }

        [Fact]
        public void FxCopConverter_CreateResult_FakeContext_NoModule_Member()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineTarget(@"mybinary.dll");
            context.RefineNamespace("mynamespace");
            context.RefineType("mytype");
            context.RefineMember("mymember(string)");
            context.RefineMessage("CA0000", "VeryUsefulCheck", null, null, null, null);
            context.RefineIssue("hello!", null, null, null, null, null, null);

            var expectedLogicalLocations = new List<LogicalLocation>
            {

                    new LogicalLocation { ParentIndex = -1, Name = "mynamespace", Kind = LogicalLocationKind.Namespace },
                    new LogicalLocation { ParentIndex =  0, Name = "mytype",    FullyQualifiedName = "mynamespace.mytype", Kind = LogicalLocationKind.Type },
                    new LogicalLocation { ParentIndex =  1, Name = "mymember(string)", FullyQualifiedName = "mynamespace.mytype.mymember(string)", Kind = LogicalLocationKind.Member }
            };

            var converter = new FxCopConverter();
            Result result = converter.CreateResult(context);

            ValidateLogicalLocations(expectedLogicalLocations, converter.LogicalLocations);
        }

        [Fact]
        public void FxCopConverter_CreateResult_FakeContext_Resource()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineTarget(@"mybinary.dll");
            context.RefineModule("mybinary.dll");
            context.RefineResource("myresource.resx");
            context.RefineMessage("CA0000", "VeryUsefulCheck", null, null, null, null);
            context.RefineIssue("hello!", "test", null, null, @"source", "myfile.cs", 13);

            var expectedLogicalLocations = new List<LogicalLocation>
            {
                new LogicalLocation { Kind = LogicalLocationKind.Module, Name = "mybinary.dll" },
                new LogicalLocation { ParentIndex = 0, Name = "myresource.resx", FullyQualifiedName = "mybinary.dll!myresource.resx", Kind = LogicalLocationKind.Resource }
            };

            var converter = new FxCopConverter();
            Result result = converter.CreateResult(context);

            ValidateLogicalLocations(expectedLogicalLocations, converter.LogicalLocations);
        }

        [Fact]
        public void FxCopConverter_CreateResult_FakeContext_NoModule_Resource()
        {
            FxCopLogReader.Context context = TestHelper.CreateProjectContext();

            context.RefineTarget(@"mybinary.dll");
            context.RefineResource("myresource.resx");
            context.RefineMessage("CA0000", "VeryUsefulCheck", null, null, null, null);
            context.RefineIssue("hello!", "test", null, null, null, null, null);

            var converter = new FxCopConverter();
            Result result = converter.CreateResult(context);

            result.Locations.First().LogicalLocation.FullyQualifiedName.Should().Be(@"myresource.resx");
        }
    }

    internal static class TestHelper
    {
        // fake fxcop project location
        public static FxCopLogReader.Context CreateProjectContext()
        {
            return new FxCopLogReader.Context();
        }
    }

    internal static class FxCopTestData
    {
        public static string FxCopReportInvalid =
@"<?xml-stylesheet type=""text/xsl"" href=""c:\program files (x86)\microsoft\fxcop 12.0 for sdl 6.1\Xml\FxCopReport.xsl""?>
<FxCopReport Version=""12.0"">";

        public static string FxCopReportEmpty =
@"<?xml-stylesheet type=""text/xsl"" href=""c:\program files (x86)\microsoft\fxcop 12.0 for sdl 6.1\Xml\FxCopReport.xsl""?>
<FxCopReport Version=""12.0"">
</FxCopReport>";

        public static string FxCopReportEmptyExtraHeader =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<?xml-stylesheet type=""text/xsl"" href=""c:\program files (x86)\microsoft\fxcop 12.0 for sdl 6.1\Xml\FxCopReport.xsl""?>
<FxCopReport Version=""12.0"">
</FxCopReport>";

        public static string FxCopReportBadStartTag = @"<FxCopReportBad></FxCopReportBad>";
        public static string FxCopReportBadXml = @"<FxCopReport></FxCopReportBad>";
    }
}
