// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Xml;
using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Writers;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Converters
{
    [TestClass]
    public class AndroidStudioConverterTests
    {
        private AndroidStudioConverter _converter = null;

        [TestInitialize]
        public void Initialize()
        {
            _converter = new AndroidStudioConverter();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AndroidStudioConverter_Convert_Nulls()
        {
            _converter.Convert(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AndroidStudioConverter_Convert_NullInput()
        {
            _converter.Convert(null, new ResultLogObjectWriter());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AndroidStudioConverter_Convert_NullOutput()
        {
            _converter.Convert(new MemoryStream(), null);
        }

        private const string EmptyResult = @"{
  ""version"": ""0.4"",
  ""runLogs"": [
    {
      ""toolInfo"": {
        ""name"": ""AndroidStudio""
      },
      ""results"": []
    }
  ]
}";
        [TestMethod]
        public void AndroidStudioConverter_Convert_NoIssues()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            Assert.AreEqual(EmptyResult, actualJson);
        }

        [TestMethod]
        public void AndroidStudioConverter_Convert_EmptyIssue()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem></problem></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            Assert.AreEqual(EmptyResult, actualJson);
        }

        [TestMethod]
        public void AndroidStudioConverter_Convert_EmptyIssueSingleTag()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem /></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            Assert.AreEqual(EmptyResult, actualJson);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void AndroidStudioConverter_Convert_InvalidLineNumber()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem><file>file://$PROJECT_DIR$/northwindtraders-droid-2/src/main/java/com/northwindtraders/droid/model/Model.java</file><line>NotALineNumber</line><module>northwindtraders-droid-2</module><package>com.northwindtraders.droid.model</package><entry_point TYPE=""file"" FQNAME=""file://$PROJECT_DIR$/northwindtraders-droid-2/src/main/java/com/northwindtraders/droid/model/Model.java""/><problem_class severity=""WARNING"" attribute_key=""WARNING_ATTRIBUTES"">Assertions</problem_class><hints/><description>Assertions are unreliable. Use BuildConfig.DEBUG conditional checks instead.</description></problem></problems>";
            Utilities.GetConverterJson(_converter, androidStudioLog);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void AndroidStudioConverter_Convert_NonXmlNodeTypeText_ChildElements()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem><file><child_file>file://$PROJECT_DIR$/</child_file></file><line>1</line><module>northwindtraders-droid-2</module><package>com.northwindtraders.droid.model</package><entry_point TYPE=""file"" FQNAME=""file://$PROJECT_DIR$/northwindtraders-droid-2/src/main/java/com/northwindtraders/droid/model/Model.java""/><problem_class severity=""WARNING"" attribute_key=""WARNING_ATTRIBUTES"">Assertions</problem_class><hints/><description>Assertions are unreliable. Use BuildConfig.DEBUG conditional checks instead.</description></problem></problems>";
            Utilities.GetConverterJson(_converter, androidStudioLog);
        }

        [TestMethod]
        public void AndroidStudioConverter_GetShortDescription_UsesDescriptionIfPresent()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Description = "Cute fluffy kittens";
            var uut = new AndroidStudioProblem(builder);
            string result = AndroidStudioConverter.GetShortDescriptionForProblem(uut);
            Assert.AreEqual("Cute fluffy kittens", result);
        }

        [TestMethod]
        public void AndroidStudioConverter_GetShortDescription_UsesProblemClassIfDescriptionNotPresent()
        {
            var uut = AndroidStudioProblemTests.GetDefaultProblem();
            string result = AndroidStudioConverter.GetShortDescriptionForProblem(uut);
            result.Should().Contain("A Problematic Problem");
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifIssue_EmptyHintsDoNotAffectDescription()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Description = "hungry EVIL zombies";
            var uut = new AndroidStudioProblem(builder);
            Result result = AndroidStudioConverter.ConvertProblemToSarifIssue(uut);
            Assert.IsNull(result.ShortMessage);
            Assert.AreEqual("hungry EVIL zombies", result.FullMessage);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifIssue_HintsAreStapledToDescription()
        {
            var builder = new AndroidStudioProblem.Builder
            {
                ProblemClass = "Unused",
                File = "Unused",
                Description = "hungry EVIL zombies",
                Hints = ImmutableArray.Create("comment", "delete")
            };

            var uut = new AndroidStudioProblem(builder);
            Result result = AndroidStudioConverter.ConvertProblemToSarifIssue(uut);
            Assert.AreEqual("hungry EVIL zombies", result.ShortMessage);
            Assert.AreEqual(@"hungry EVIL zombies
Possible resolution: comment
Possible resolution: delete", result.FullMessage);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifIssue_UsesProblemClassForRuleId()
        {
            var uut = AndroidStudioProblemTests.GetDefaultProblem();
            Result result = AndroidStudioConverter.ConvertProblemToSarifIssue(uut);
            Assert.AreEqual("A Problematic Problem", result.RuleId);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifIssue_HasNoPropertiesIfAttributeKeyAndSeverity()
        {
            var uut = AndroidStudioProblemTests.GetDefaultProblem();
            Result result = AndroidStudioConverter.ConvertProblemToSarifIssue(uut);
            Assert.IsNull(result.Properties);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifIssue_AttributeKeyIsPersistedInProperties()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.AttributeKey = "key";
            var uut = new AndroidStudioProblem(builder);
            Result result = AndroidStudioConverter.ConvertProblemToSarifIssue(uut);
            result.Properties.Should().Equal(new Dictionary<string, string> {
                {"attributeKey", "key"}
            });
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifIssue_SeverityIsPersistedInProperties()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Severity = "warning";
            var uut = new AndroidStudioProblem(builder);
            Result result = AndroidStudioConverter.ConvertProblemToSarifIssue(uut);
            result.Properties.Should().Equal(new Dictionary<string, string> {
                {"severity", "warning"}
            });
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifIssue_MultiplePropertiesArePersisted()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.AttributeKey = "key";
            builder.Severity = "warning";
            var uut = new AndroidStudioProblem(builder);
            Result result = AndroidStudioConverter.ConvertProblemToSarifIssue(uut);
            result.Properties.Should().Equal(new Dictionary<string, string> {
                {"severity", "warning"},
                {"attributeKey", "key"}
            });
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifIssue_RecordsTopLevelFileAsSourceFile()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "expected_file.java";
            builder.EntryPointType = "file";
            builder.EntryPointName = "bad_file.java";
            Location loc = GetLocationForBuilder(builder);
            Assert.AreEqual("expected_file.java", loc.IssueFile[0].Uri.ToString());
            Assert.AreSame(MimeType.Java, loc.IssueFile[0].MimeType);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifIssue_DoesNotRecordTopLevelEntryPointAsSourceFile()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.EntryPointType = "file";
            builder.EntryPointName = "expected_file.java";
            Location loc = GetLocationForBuilder(builder);
            Assert.IsNull(loc.IssueFile);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifIssue_RecordsModuleAsTopLevelIfPresent()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Module = "my_fancy_binary";
            builder.EntryPointType = "method";
            builder.EntryPointName = "my_method";
            Assert.AreEqual(
                new Location
                {
                    FullyQualifiedLogicalName = "my_fancy_binary\\my_method",
                    LogicalLocation = new[]
                    {
                        new LogicalLocationComponent
                        {
                            Name = "my_fancy_binary",
                            Kind = LogicalLocationKind.AndroidModule
                        },
                        new LogicalLocationComponent
                        {
                            Name = "my_method",
                            Kind = LogicalLocationKind.JvmFunction
                        }
                    }
                }
                , GetLocationForBuilder(builder));
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifIssue_GeneratesLocationWithOnlyMethodEntryPoint()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Package = null;
            builder.Module = null;
            builder.EntryPointType = "method";
            builder.EntryPointName = "my_method";
            Assert.AreEqual(new Location
            {
                FullyQualifiedLogicalName = "my_method",
                LogicalLocation = new[]
                {
                    new LogicalLocationComponent
                    {
                        Name = "my_method",
                        Kind = LogicalLocationKind.JvmFunction
                    }
                }
            }, GetLocationForBuilder(builder));
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifIssue_GeneratesLocationWithMethodEntryPointAndPackage()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Package = "FancyPackageName";
            builder.Module = null;
            builder.EntryPointType = "method";
            builder.EntryPointName = "my_method";
            Assert.AreEqual(new Location
            {
                FullyQualifiedLogicalName = "FancyPackageName\\my_method",
                LogicalLocation = new[]
                {
                    new LogicalLocationComponent
                    {
                        Name = "FancyPackageName",
                        Kind = LogicalLocationKind.JvmPackage
                    },
                    new LogicalLocationComponent
                    {
                        Name = "my_method",
                        Kind = LogicalLocationKind.JvmFunction
                    }
                }
            }, GetLocationForBuilder(builder));
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifIssue_GeneratesLocationWithOnlyPackage()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Package = "FancyPackageName";
            builder.Module = null;
            builder.EntryPointName = null;
            Assert.AreEqual(new Location
            {
                FullyQualifiedLogicalName = "FancyPackageName",
                LogicalLocation = new[]
                {
                    new LogicalLocationComponent
                    {
                        Name = "FancyPackageName",
                        Kind = LogicalLocationKind.JvmPackage
                    }
                }
            }, GetLocationForBuilder(builder));
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifIssue_CanRecordSourceFileAndModule()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "File Goes Here";
            builder.Package = null;
            builder.Module = "LastResortModule";
            builder.EntryPointName = null;
            Assert.AreEqual(new Location
            {
                IssueFile = new[]
                {
                    new PhysicalLocationComponent
                    {
                        Uri = new Uri("File Goes Here", UriKind.RelativeOrAbsolute),
                        MimeType = MimeType.Java
                    }
                },
                FullyQualifiedLogicalName = "LastResortModule",
                LogicalLocation = new[]
                {
                    new LogicalLocationComponent
                    {
                        Name = "LastResortModule",
                        Kind = LogicalLocationKind.AndroidModule
                    }
                }
            }, GetLocationForBuilder(builder));
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifIssue_RemovesProjectDirPrefix()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "file://$PROJECT_DIR$/mydir/myfile.xml";
            Location loc = GetLocationForBuilder(builder);
            Assert.AreEqual("mydir/myfile.xml", loc.IssueFile[0].Uri.ToString());
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifIssue_PersistsSourceLineInfo()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Line = 42;
            Location loc = GetLocationForBuilder(builder);
            Assert.AreEqual(42, loc.IssueFile[0].Region.StartLine);
        }

        private static Location GetLocationForBuilder(AndroidStudioProblem.Builder builder)
        {
            return AndroidStudioConverter.ConvertProblemToSarifIssue(new AndroidStudioProblem(builder)).Locations[0];
        }
    }
}
