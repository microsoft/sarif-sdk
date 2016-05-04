// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
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
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0-beta.4"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""AndroidStudio""
      },
      ""results"": []
    }
  ]
}";
        [TestMethod]
        public void AndroidStudioConverter_Convert_NoResults()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            Assert.AreEqual(EmptyResult, actualJson);
        }

        [TestMethod]
        public void AndroidStudioConverter_Convert_EmptyResult()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem></problem></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            Assert.AreEqual(EmptyResult, actualJson);
        }

        [TestMethod]
        public void AndroidStudioConverter_Convert_EmptyResultSingleTag()
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
        public void AndroidStudioConverter_ConvertToSarifResult_EmptyHintsDoNotAffectDescription()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Description = "hungry EVIL zombies";
            var uut = new AndroidStudioProblem(builder);

            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            Assert.AreEqual("hungry EVIL zombies", result.Message);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifResult_HintsAreStapledToDescription()
        {
            var builder = new AndroidStudioProblem.Builder
            {
                ProblemClass = "Unused",
                File = "Unused",
                Description = "hungry EVIL zombies",
                Hints = ImmutableArray.Create("comment", "delete")
            };

            var uut = new AndroidStudioProblem(builder);
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            Assert.AreEqual(@"hungry EVIL zombies
Possible resolution: comment
Possible resolution: delete", result.Message);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifResult_UsesProblemClassForRuleId()
        {
            var uut = AndroidStudioProblemTests.GetDefaultProblem();
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            Assert.AreEqual("A Problematic Problem", result.RuleId);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifResult_HasNoPropertiesIfAttributeKeyAndSeverity()
        {
            var uut = AndroidStudioProblemTests.GetDefaultProblem();
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            Assert.IsNull(result.Properties);
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifResult_AttributeKeyIsPersistedInProperties()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.AttributeKey = "key";
            var uut = new AndroidStudioProblem(builder);
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            result.Properties.Should().Equal(new Dictionary<string, string> {
                {"attributeKey", "key"}
            });
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifResult_SeverityIsPersistedInProperties()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Severity = "warning";
            var uut = new AndroidStudioProblem(builder);
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            result.Properties.Should().Equal(new Dictionary<string, string> {
                {"severity", "warning"}
            });
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertToSarifResult_MultiplePropertiesArePersisted()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.AttributeKey = "key";
            builder.Severity = "warning";
            var uut = new AndroidStudioProblem(builder);
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            result.Properties.Should().Equal(new Dictionary<string, string> {
                {"severity", "warning"},
                {"attributeKey", "key"}
            });
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifResult_RecordsTopLevelFileAsSourceFile()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "expected_file.java";
            builder.EntryPointType = "file";
            builder.EntryPointName = "bad_file.java";
            Location loc = GetLocationInfoForBuilder(builder).Location;
            loc.ResultFile.Uri.ToString().Should().Be("expected_file.java");
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifResult_DoesNotRecordTopLevelEntryPointAsSourceFile()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.EntryPointType = "file";
            builder.EntryPointName = "expected_file.java";
            Location loc = GetLocationInfoForBuilder(builder).Location;
            loc.ResultFile.Should().BeNull();
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifResult_RecordsModuleAsTopLevelIfPresent()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Module = "my_fancy_binary";
            builder.EntryPointType = "method";
            builder.EntryPointName = "my_method";

            var expectedLocation = new Location
            {
                FullyQualifiedLogicalName = "my_fancy_binary\\my_method",
            };

            var expectedLogicalLocationComponents = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "my_fancy_binary",
                    Kind = LogicalLocationKind.Module
                },
                new LogicalLocationComponent
                {
                    Name = "my_method",
                    Kind = LogicalLocationKind.Member
                }
            };

            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);

            locationInfo.Location.ValueEquals(expectedLocation).Should().BeTrue();

            locationInfo.LogicalLocationComponents
                .SequenceEqual(
                    expectedLogicalLocationComponents,
                    LogicalLocationComponent.ValueComparer)
                .Should().BeTrue();
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifResult_GeneratesLocationWithOnlyMethodEntryPoint()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Package = null;
            builder.Module = null;
            builder.EntryPointType = "method";
            builder.EntryPointName = "my_method";

            var expectedLocation = new Location
            {
                FullyQualifiedLogicalName = "my_method"
            };

            var expectedLogicalLocationComponents = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "my_method",
                    Kind = LogicalLocationKind.Member
                }
            };

            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);

            locationInfo.Location.ValueEquals(expectedLocation).Should().BeTrue();

            locationInfo.LogicalLocationComponents
                .SequenceEqual(
                    expectedLogicalLocationComponents,
                    LogicalLocationComponent.ValueComparer)
                .Should().BeTrue();
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifResult_GeneratesLocationWithMethodEntryPointAndPackage()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Package = "FancyPackageName";
            builder.Module = null;
            builder.EntryPointType = "method";
            builder.EntryPointName = "my_method";

            var expectedLocation = new Location
            {
                FullyQualifiedLogicalName = "FancyPackageName\\my_method"
            };

            var expectedLogicalLocationComponents = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "FancyPackageName",
                    Kind = LogicalLocationKind.Package
                },
                new LogicalLocationComponent
                {
                    Name = "my_method",
                    Kind = LogicalLocationKind.Member
                }
            };

            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);

            locationInfo.Location.ValueEquals(expectedLocation).Should().BeTrue();

            locationInfo.LogicalLocationComponents
                .SequenceEqual(
                    expectedLogicalLocationComponents,
                    LogicalLocationComponent.ValueComparer)
                .Should().BeTrue();
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifResult_GeneratesLocationWithOnlyPackage()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Package = "FancyPackageName";
            builder.Module = null;
            builder.EntryPointName = null;

            var expectedLocation = new Location
            {
                FullyQualifiedLogicalName = "FancyPackageName"
            };

            var expectedLogicalLocationComponents = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "FancyPackageName",
                    Kind = LogicalLocationKind.Package
                }
            };

            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);

            locationInfo.Location.ValueEquals(expectedLocation).Should().BeTrue();

            locationInfo.LogicalLocationComponents
                .SequenceEqual(
                    expectedLogicalLocationComponents,
                    LogicalLocationComponent.ValueComparer)
                .Should().BeTrue();
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifResult_CanRecordSourceFileAndModule()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "File Goes Here";
            builder.Package = null;
            builder.Module = "LastResortModule";
            builder.EntryPointName = null;

            var expectedLocation = new Location
            {
                ResultFile = new PhysicalLocation
                {
                    Uri = new Uri("File Goes Here", UriKind.RelativeOrAbsolute),
                },
                FullyQualifiedLogicalName = "LastResortModule"
            };

            var expectedLogicalLocationComponents = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "LastResortModule",
                    Kind = LogicalLocationKind.Module
                }
            };

            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);

            locationInfo.Location.ValueEquals(expectedLocation).Should().BeTrue();

            locationInfo.LogicalLocationComponents
                .SequenceEqual(
                    expectedLogicalLocationComponents,
                    LogicalLocationComponent.ValueComparer)
                .Should().BeTrue();
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifResult_RemovesProjectDirPrefix()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "file://$PROJECT_DIR$/mydir/myfile.xml";
            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);
            locationInfo.Location.ResultFile.Uri.ToString().Should().Be("mydir/myfile.xml");
        }

        [TestMethod]
        public void AndroidStudioConverter_ConvertSarifResult_PersistsSourceLineInfo()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Line = 42;
            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);
            locationInfo.Location.ResultFile.Region.StartLine.Should().Be(42);
        }

        private struct LocationInfo
        {
            public Location Location;
            public IList<LogicalLocationComponent> LogicalLocationComponents;
        }

        private static LocationInfo GetLocationInfoForBuilder(AndroidStudioProblem.Builder builder)
        {
            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            Location location = result.Locations.First();

            string logicalLocationKey = converter.LogicalLocationsDictionary.Keys.SingleOrDefault();
            IList<LogicalLocationComponent> logicalLocationComponents = logicalLocationKey != null
                ? converter.LogicalLocationsDictionary[logicalLocationKey]
                : new List<LogicalLocationComponent>(0);

            return new LocationInfo
            {
                Location = location,
                LogicalLocationComponents = logicalLocationComponents
            };
        }
    }
}
