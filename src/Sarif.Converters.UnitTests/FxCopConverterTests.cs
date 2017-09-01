// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    [TestClass]
    public class FxCopLogReaderTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FxCopLogReader_Read_NullContext_NullInput()
        {
            new FxCopLogReader().Read(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FxCopLogReader_Read_NullInput()
        {
            var context = new FxCopLogReader.Context();
            new FxCopLogReader().Read(context, null);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Xml.XmlException))]
        public void FxCopLogReader_Read_BadStartTag()
        {
            using (MemoryStream input = Utilities.CreateStreamFromString(FxCopTestData.FxCopReportBadStartTag))
            {
                var context = new FxCopLogReader.Context();
                new FxCopLogReader().Read(context, input);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.Xml.XmlException))]
        public void FxCopLogReader_Read_BadXml()
        {
            using (MemoryStream input = Utilities.CreateStreamFromString(FxCopTestData.FxCopReportBadXml))
            {
                var context = new FxCopLogReader.Context();
                new FxCopLogReader().Read(context, input);
            }
        }
    }

    [TestClass]
    public class FxCopLogReader_ContextTests
    {
        [TestMethod]
        public void FxCopLogReader_Context_RefineReport()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineReport("myreport");
            Assert.AreEqual("myreport", context.Report);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineResource()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineResource("myresource.resx");
            Assert.AreEqual("myresource.resx", context.Resource);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineException()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineException(true, "CA0001", "mytarget");
            Assert.IsTrue(context.Exception);
            Assert.AreEqual("CA0001", context.CheckId);
            Assert.AreEqual("mytarget", context.ExceptionTarget);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineExceptionType()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineExceptionType("mytype");
            Assert.AreEqual("mytype", context.ExceptionType);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineExceptionMessage()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineExceptionMessage("mymessage");
            Assert.AreEqual("mymessage", context.ExceptionMessage);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineStackTrace()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineStackTrace(@"trace\n trace");
            Assert.AreEqual(@"trace\n trace", context.StackTrace);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineInnerExceptionType()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineInnerExceptionType(@"myinnertype");
            Assert.AreEqual(@"myinnertype", context.InnerExceptionType);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineInnerExceptionMessage()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineInnerExceptionMessage(@"myinnermessage");
            Assert.AreEqual(@"myinnermessage", context.InnerExceptionMessage);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineInnerStackTrace()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineInnerStackTrace(@"myinnertrace");
            Assert.AreEqual(@"myinnertrace", context.InnerStackTrace);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineTarget()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineTarget("mybinary.dll");
            Assert.AreEqual("mybinary.dll", context.Target);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineModule()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineModule("mybinary.dll");
            Assert.AreEqual("mybinary.dll", context.Module);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineNamespace()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineNamespace("mynamespace");
            Assert.AreEqual("mynamespace", context.Namespace);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineType()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineType("mytype");
            Assert.AreEqual("mytype", context.Type);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineMember()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineMember("mymember(string)");
            Assert.AreEqual("mymember(string)", context.Member);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineMessage()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineMessage("CA0000", "VeryUsefulCheck", "1", "MyCategory", "Breaking", "ExcludedInSource");
            Assert.AreEqual("CA0000", context.CheckId);
            Assert.AreEqual("1", context.MessageId);
            Assert.AreEqual("MyCategory", context.Category);
            Assert.AreEqual("VeryUsefulCheck", context.Typename);
            Assert.AreEqual("Breaking", context.FixCategory);
            Assert.AreEqual("ExcludedInSource", context.Status);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineIssue()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineIssue("hello!", "test", "25", "error", "source", "myfile.cs", 13);
            Assert.AreEqual("hello!", context.Message);
            Assert.AreEqual("test", context.Result);
            Assert.AreEqual("25", context.Certainty);
            Assert.AreEqual("error", context.Level);
            Assert.AreEqual("source", context.Path);
            Assert.AreEqual("myfile.cs", context.File);
            Assert.AreEqual(13, context.Line.Value);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineProjectToMemberIssue()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineTarget("mybinary.dll");
            context.RefineModule("mybinary.dll");
            context.RefineNamespace("mynamespace");
            context.RefineType("mytype");
            context.RefineMember("mymember(string)");
            context.RefineMessage("CA0000", "VeryUsefulCheck", "1", "MyCategory", "Breaking", "Excluded");
            context.RefineIssue("hello!", "test", "25", "error", "source", "myfile.cs", 13);

            Assert.AreEqual("mybinary.dll", context.Target);
            Assert.AreEqual("mybinary.dll", context.Module);
            Assert.AreEqual("mynamespace", context.Namespace);
            Assert.AreEqual("mytype", context.Type);
            Assert.AreEqual("mymember(string)", context.Member);
            Assert.AreEqual("CA0000", context.CheckId);
            Assert.AreEqual("1", context.MessageId);
            Assert.AreEqual("MyCategory", context.Category);
            Assert.AreEqual("VeryUsefulCheck", context.Typename);
            Assert.AreEqual("Breaking", context.FixCategory);
            Assert.AreEqual("hello!", context.Message);
            Assert.AreEqual("test", context.Result);
            Assert.AreEqual("25", context.Certainty);
            Assert.AreEqual("error", context.Level);
            Assert.AreEqual("source", context.Path);
            Assert.AreEqual("myfile.cs", context.File);
            Assert.AreEqual("Excluded", context.Status);
            Assert.AreEqual(13, context.Line.Value);

            context.ClearTarget();
            Assert.IsNull(context.Target);
            Assert.IsNull(context.Module);
            Assert.IsNull(context.Namespace);
            Assert.IsNull(context.Type);
            Assert.IsNull(context.Member);
            Assert.IsNull(context.Message);
            Assert.IsNull(context.Result);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineProjectToResourceIssue()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineTarget("mybinary.dll");
            context.RefineModule("mybinary.dll");
            context.RefineResource("myresource.resx");
            context.RefineMessage("CA0000", "VeryUsefulCheck", "1", "MyCategory", "Breaking", null);

            context.RefineIssue("hello!", "test", "25", "error", "source", "myresource.resx", 13);
            Assert.AreEqual("mybinary.dll", context.Target);
            Assert.AreEqual("mybinary.dll", context.Module);
            Assert.AreEqual("myresource.resx", context.Resource);
            Assert.AreEqual("CA0000", context.CheckId);
            Assert.AreEqual("1", context.MessageId);
            Assert.AreEqual("MyCategory", context.Category);
            Assert.AreEqual("VeryUsefulCheck", context.Typename);
            Assert.AreEqual("Breaking", context.FixCategory);
            Assert.AreEqual("hello!", context.Message);
            Assert.AreEqual("test", context.Result);
            Assert.AreEqual("25", context.Certainty);
            Assert.AreEqual("error", context.Level);
            Assert.AreEqual("source", context.Path);
            Assert.AreEqual(null, context.Status);
            Assert.AreEqual("myresource.resx", context.File);
            Assert.AreEqual(13, context.Line.Value);

            context.ClearTarget();
            Assert.IsNull(context.Target);
            Assert.IsNull(context.Module);
            Assert.IsNull(context.Resource);
            Assert.IsNull(context.Namespace);
            Assert.IsNull(context.Type);
            Assert.IsNull(context.Member);
            Assert.IsNull(context.Message);
            Assert.IsNull(context.Result);
        }

        [TestMethod]
        public void FxCopLogReader_Context_RefineProjectToExceptionIssue()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineException(true, "CA0001", "binary.dll#namespace#member(string)");
            context.RefineExceptionType("type");
            context.RefineExceptionMessage("message");
            context.RefineStackTrace("trace");
            context.RefineInnerExceptionType("innertype");
            context.RefineInnerExceptionMessage("innermessage");
            context.RefineInnerStackTrace("innertrace");

            string exception = FxCopLogReader.MakeExceptionMessage("Rule", "CA1000", context.ExceptionType, context.ExceptionMessage, context.StackTrace, context.InnerExceptionType, context.InnerExceptionMessage, context.InnerStackTrace);
            context.RefineIssue(exception, null, null, null, null, null, null);

            Assert.AreEqual("Rule CA1000 exception: type: message trace. Inner Exception: innertype: innermessage innertrace", context.Message);
            Assert.AreEqual("binary.dll#namespace#member(string)", context.ExceptionTarget);

            context.ClearException();
            Assert.IsNull(context.Target);
            Assert.IsNull(context.Module);
            Assert.IsNull(context.Namespace);
            Assert.IsNull(context.Type);
            Assert.IsNull(context.Member);
            Assert.IsNull(context.Message);
            Assert.IsNull(context.Result);

            Assert.IsFalse(context.Exception);
            Assert.IsNull(context.ExceptionType);
            Assert.IsNull(context.ExceptionMessage);
            Assert.IsNull(context.StackTrace);
            Assert.IsNull(context.InnerExceptionType);
            Assert.IsNull(context.InnerExceptionMessage);
            Assert.IsNull(context.InnerStackTrace);
        }
    }

    [TestClass]
    public class FxCopConverterTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FxCopConverter_Convert_NullInput()
        {
            var converter = new FxCopConverter();
            converter.Convert(null, null, LoggingOptions.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FxCopConverter_Convert_NullOutput()
        {
            var converter = new FxCopConverter();
            converter.Convert(new MemoryStream(), null, LoggingOptions.None);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Xml.XmlException))]
        public void FxCopConverter_Convert_InvalidInput()
        {
            var converter = new FxCopConverter();
            Utilities.GetConverterJson(converter, FxCopTestData.FxCopReportInvalid);
        }

        [TestMethod]
        public void FxCopConverter_CreateResult_FakeContext_Member()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineTarget(@"mybinary.dll");
            context.RefineModule("mybinary.dll");
            context.RefineNamespace("mynamespace");
            context.RefineType("mytype");
            context.RefineMember("mymember(string)");
            context.RefineMessage("CA0000", "VeryUsefulCheck", "1", "FakeCategory", "Breaking", "ExcludedInSource");
            context.RefineIssue("hello!", "test", "uncertain", "error", @"source", "myfile.cs", 13);

            string expectedLogicalLocation = "mynamespace.mytype.mymember(string)";

            var expectedResult = new Result
            {
                RuleId = "CA0000",
                Message = "hello!",
                ToolFingerprintContribution = "1#test",
                SuppressionStates = SuppressionStates.SuppressedInSource,
                Locations = new List<Location>
                {
                    new Location
                    {
                        AnalysisTarget = new PhysicalLocation
                        {
                            Uri = new Uri("mybinary.dll", UriKind.RelativeOrAbsolute),
                        },
                        ResultFile = new PhysicalLocation
                        {
                            Uri = new Uri("source\\myfile.cs", UriKind.RelativeOrAbsolute),
                            Region = new Region { StartLine = 13 }
                        },
                        FullyQualifiedLogicalName = expectedLogicalLocation,
                    }
                }
            };

            expectedResult.SetProperty("Level", "error");
            expectedResult.SetProperty("Category", "FakeCategory");
            expectedResult.SetProperty("FixCategory", "Breaking");

            var expectedLogicalLocations = new Dictionary<string, LogicalLocation>
            {
                {
                    "mybinary.dll", new LogicalLocation { ParentKey = null, Name = "mybinary.dll", Kind = LogicalLocationKind.Module }
                },
                {
                    "mybinary.dll!mynamespace",
                    new LogicalLocation { ParentKey = "mybinary.dll", Name = "mynamespace", Kind = LogicalLocationKind.Namespace }
                },
                {
                    "mybinary.dll!mynamespace.mytype",
                    new LogicalLocation { ParentKey = "mybinary.dll!mynamespace", Name = "mytype", Kind = LogicalLocationKind.Type }
                },
                {
                    "mybinary.dll!mynamespace.mytype.mymember(string)",
                    new LogicalLocation { ParentKey = "mybinary.dll!mynamespace.mytype", Name = "mymember(string)", Kind = LogicalLocationKind.Member }
                }            };

            var converter = new FxCopConverter();
            Result result = converter.CreateResult(context);

            foreach (string key in expectedLogicalLocations.Keys)
            {
                expectedLogicalLocations[key].ValueEquals(converter.LogicalLocationsDictionary[key]).Should().BeTrue();
            }
            converter.LogicalLocationsDictionary.Count.Should().Be(expectedLogicalLocations.Count);
        }

        [TestMethod]
        public void FxCopConverter_CreateIssue_FakeContext_NoModule_Member()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineTarget(@"mybinary.dll");
            context.RefineNamespace("mynamespace");
            context.RefineType("mytype");
            context.RefineMember("mymember(string)");
            context.RefineMessage("CA0000", "VeryUsefulCheck", null, null, null, null);
            context.RefineIssue("hello!", null, null, null, null, null, null);

            var expectedLogicalLocation = "mynamespace.mytype.mymember(string)";

            var expectedLocations = new[]
            {
                new Location
                {
                    AnalysisTarget = new PhysicalLocation
                    {
                        Uri = new Uri("mybinary.dll", UriKind.RelativeOrAbsolute),
                    },

                    FullyQualifiedLogicalName = expectedLogicalLocation
                }
            };

            var expectedLogicalLocations = new Dictionary<string, LogicalLocation>
            {
                {
                    "mynamespace",
                    new LogicalLocation { ParentKey = null, Name = "mynamespace", Kind = LogicalLocationKind.Namespace }
                },
                {
                    "mynamespace.mytype",
                    new LogicalLocation { ParentKey = "mynamespace", Name = "mytype", Kind = LogicalLocationKind.Type }
                },
                {
                    "mynamespace.mytype.mymember(string)",
                    new LogicalLocation { ParentKey = "mynamespace.mytype", Name = "mymember(string)", Kind = LogicalLocationKind.Member }
                }
            };

            var converter = new FxCopConverter();
            Result result = converter.CreateResult(context);

            foreach (string key in expectedLogicalLocations.Keys)
            {
                expectedLogicalLocations[key].ValueEquals(converter.LogicalLocationsDictionary[key]).Should().BeTrue();
            }
            converter.LogicalLocationsDictionary.Count.Should().Be(expectedLogicalLocations.Count);
        }

        [TestMethod]
        public void FxCopConverter_CreateResult_FakeContext_Resource()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineTarget(@"mybinary.dll");
            context.RefineModule("mybinary.dll");
            context.RefineResource("myresource.resx");
            context.RefineMessage("CA0000", "VeryUsefulCheck", null, null, null, null);
            context.RefineIssue("hello!", "test", null, null, @"source", "myfile.cs", 13);

            var expectedLogicalLocation = "myresource.resx";

            var expectedLocations = new[]
            {
                new Location
                {
                    AnalysisTarget = new PhysicalLocation
                    {
                        Uri = new Uri("mybinary.dll", UriKind.RelativeOrAbsolute),
                    },
                    ResultFile = new PhysicalLocation
                    {
                            Uri = new Uri("source\\myfile.cs", UriKind.RelativeOrAbsolute),
                            Region = new Region { StartLine = 13 }
                    },
                    FullyQualifiedLogicalName = expectedLogicalLocation,
                }
            };

            var expectedLogicalLocations = new Dictionary<string, LogicalLocation>
            {
                {
                    "mybinary.dll",
                    new LogicalLocation { ParentKey = null, Name = "mybinary.dll", Kind = LogicalLocationKind.Module }
                },
                {
                    "mybinary.dll!myresource.resx",
                    new LogicalLocation { ParentKey = "mybinary.dll", Name = "myresource.resx",Kind = LogicalLocationKind.Resource }
                },
            };

            var converter = new FxCopConverter();
            Result result = converter.CreateResult(context);

            foreach (string key in expectedLogicalLocations.Keys)
            {
                expectedLogicalLocations[key].ValueEquals(converter.LogicalLocationsDictionary[key]).Should().BeTrue();
            }
            converter.LogicalLocationsDictionary.Count.Should().Be(expectedLogicalLocations.Count);
        }

        [TestMethod]
        public void FxCopConverter_CreateResult_FakeContext_NoModule_Resource()
        {
            var context = TestHelper.CreateProjectContext();

            context.RefineTarget(@"mybinary.dll");
            context.RefineResource("myresource.resx");
            context.RefineMessage("CA0000", "VeryUsefulCheck", null, null, null, null);
            context.RefineIssue("hello!", "test", null, null, null, null, null);

            var converter = new FxCopConverter();
            Result result = converter.CreateResult(context);

            result.Locations.First().FullyQualifiedLogicalName.Should().Be(@"myresource.resx");
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
