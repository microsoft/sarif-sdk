// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Writers;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class AndroidStudioConverterTests : ConverterTestsBase<AndroidStudioConverter>
    {
        private readonly AndroidStudioConverter _converter = null;

        public AndroidStudioConverterTests()
        {
            _converter = new AndroidStudioConverter();
        }

        [Fact]
        public void AndroidStudioConverter_Convert_Nulls()
        {
            Assert.Throws<ArgumentNullException>(() => _converter.Convert(null, null, OptionallyEmittedData.None));
        }

        [Fact]
        public void AndroidStudioConverter_Convert_NullInput()
        {
            Assert.Throws<ArgumentNullException>(() => _converter.Convert(null, new ResultLogObjectWriter(), OptionallyEmittedData.None));
        }

        [Fact]
        public void AndroidStudioConverter_Convert_NullOutput()
        {
            Assert.Throws<ArgumentNullException>(() => _converter.Convert(new MemoryStream(), null, OptionallyEmittedData.None));
        }

        [Fact]
        public void AndroidStudioConverter_Convert_NoResults()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            actualJson.Should().BeCrossPlatformEquivalent<SarifLog>(EmptyResultLogText);
        }

        [Fact]
        public void AndroidStudioConverter_Convert_EmptyResult()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem></problem></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            actualJson.Should().BeCrossPlatformEquivalent<SarifLog>(EmptyResultLogText);
        }

        [Fact]
        public void AndroidStudioConverter_Convert_EmptyResultSingleTag()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem /></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            actualJson.Should().BeCrossPlatformEquivalent<SarifLog>(EmptyResultLogText);
        }

        [Fact]
        public void AndroidStudioConverter_Convert_InvalidLineNumber()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem><file>file://$PROJECT_DIR$/northwindtraders-droid-2/src/main/java/com/northwindtraders/droid/model/Model.java</file><line>NotALineNumber</line><module>northwindtraders-droid-2</module><package>com.northwindtraders.droid.model</package><entry_point TYPE=""file"" FQNAME=""file://$PROJECT_DIR$/northwindtraders-droid-2/src/main/java/com/northwindtraders/droid/model/Model.java""/><problem_class severity=""WARNING"" attribute_key=""WARNING_ATTRIBUTES"">Assertions</problem_class><hints/><description>Assertions are unreliable. Use BuildConfig.DEBUG conditional checks instead.</description></problem></problems>";
            Assert.Throws<XmlException>(() => Utilities.GetConverterJson(_converter, androidStudioLog));
        }

        [Fact]
        public void AndroidStudioConverter_Convert_NonXmlNodeTypeText_ChildElements()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem><file><child_file>file://$PROJECT_DIR$/</child_file></file><line>1</line><module>northwindtraders-droid-2</module><package>com.northwindtraders.droid.model</package><entry_point TYPE=""file"" FQNAME=""file://$PROJECT_DIR$/northwindtraders-droid-2/src/main/java/com/northwindtraders/droid/model/Model.java""/><problem_class severity=""WARNING"" attribute_key=""WARNING_ATTRIBUTES"">Assertions</problem_class><hints/><description>Assertions are unreliable. Use BuildConfig.DEBUG conditional checks instead.</description></problem></problems>";
            Assert.Throws<XmlException>(() => Utilities.GetConverterJson(_converter, androidStudioLog));
        }

        [Fact]
        public void AndroidStudioConverter_GetShortDescription_UsesDescriptionIfPresent()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Description = "Cute fluffy kittens";
            var uut = new AndroidStudioProblem(builder);
            string result = AndroidStudioConverter.GetShortDescriptionForProblem(uut);
            Assert.Equal("Cute fluffy kittens", result);
        }

        [Fact]
        public void AndroidStudioConverter_GetShortDescription_UsesProblemClassIfDescriptionNotPresent()
        {
            AndroidStudioProblem uut = AndroidStudioProblemTests.GetDefaultProblem();
            string result = AndroidStudioConverter.GetShortDescriptionForProblem(uut);
            result.Should().Contain("A Problematic Problem");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_EmptyHintsDoNotAffectDescription()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Description = "hungry EVIL zombies";
            var uut = new AndroidStudioProblem(builder);

            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            Assert.Equal("hungry EVIL zombies", result.Message.Text);
        }

        [Fact]
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
            Assert.Equal(@"hungry EVIL zombies
Possible resolution: comment
Possible resolution: delete", result.Message.Text);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_UsesProblemClassForRuleId()
        {
            AndroidStudioProblem uut = AndroidStudioProblemTests.GetDefaultProblem();
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            Assert.Equal("A Problematic Problem", result.RuleId);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_HasNoPropertiesIfAttributeKeyAndSeverity()
        {
            AndroidStudioProblem uut = AndroidStudioProblemTests.GetDefaultProblem();
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            Assert.Null(result.Properties);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_AttributeKeyIsPersistedInProperties()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.AttributeKey = "key";
            var uut = new AndroidStudioProblem(builder);
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            result.PropertyNames.Count.Should().Be(1);
            result.GetProperty("attributeKey").Should().Be("key");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_SeverityIsPersistedInProperties()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Severity = "warning";
            var uut = new AndroidStudioProblem(builder);
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            result.PropertyNames.Count.Should().Be(1);
            result.GetProperty("severity").Should().Be("warning");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_MultiplePropertiesArePersisted()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.AttributeKey = "key";
            builder.Severity = "warning";
            var uut = new AndroidStudioProblem(builder);
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            result.PropertyNames.Count.Should().Be(2);
            result.GetProperty("severity").Should().Be("warning");
            result.GetProperty("attributeKey").Should().Be("key");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_RecordsTopLevelFileAsSourceFile()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "expected_file.java";
            builder.EntryPointType = "file";
            builder.EntryPointName = "bad_file.java";
            Location loc = GetLocationInfoForBuilder(builder).Location;
            loc.PhysicalLocation.ArtifactLocation.Uri.ToString().Should().Be("expected_file.java");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_DoesNotRecordTopLevelEntryPointAsSourceFile()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.EntryPointType = "file";
            builder.EntryPointName = "expected_file.java";
            Location loc = GetLocationInfoForBuilder(builder).Location;
            loc.PhysicalLocation.Should().BeNull();
        }

        internal static void ValidateLogicalLocations(IList<LogicalLocation> expectedLogicalLocations, IList<LogicalLocation> actualLogicalLocations)
        {
            for (int i = 0; i < expectedLogicalLocations.Count; i++)
            {
                expectedLogicalLocations[i].ValueEquals(actualLogicalLocations[i]).Should().BeTrue();
            }
            actualLogicalLocations.Count.Should().Be(expectedLogicalLocations.Count);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_RecordsModuleAsTopLevelIfPresent()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Module = "my_fancy_binary";
            builder.EntryPointType = "method";
            builder.EntryPointName = "my_method";

            var expectedLocation = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "my_fancy_binary\\my_method",
                    Index = 1
                }
            };

            var expectedLogicalLocations = new List<LogicalLocation>
            {
                new LogicalLocation { ParentIndex = -1, FullyQualifiedName = "my_fancy_binary", Kind = LogicalLocationKind.Module },
                new LogicalLocation { ParentIndex = 0, Name = "my_method", FullyQualifiedName = @"my_fancy_binary\my_method", Kind = LogicalLocationKind.Member }
           };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            ValidateLogicalLocations(expectedLogicalLocations, converter.LogicalLocations);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_GeneratesLocationWithOnlyMethodEntryPoint()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Package = null;
            builder.Module = null;
            builder.EntryPointType = "method";
            builder.EntryPointName = "my_method";

            var expectedLocation = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "my_method",
                    Index = 0
                }
            };

            var expectedLogicalLocations = new List<LogicalLocation>
            {
                new LogicalLocation { ParentIndex = -1, Kind = LogicalLocationKind.Member, FullyQualifiedName = "my_method" }
           };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            ValidateLogicalLocations(expectedLogicalLocations, converter.LogicalLocations);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_GeneratesLocationWithMethodEntryPointAndPackage()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Package = "FancyPackageName";
            builder.Module = null;
            builder.EntryPointType = "method";
            builder.EntryPointName = "my_method";

            var expectedLocation = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "FancyPackageName\\my_method",
                    Index = 1
                }
            };

            var expectedLogicalLocations = new List<LogicalLocation>
            {
                new LogicalLocation { ParentIndex = -1, FullyQualifiedName = "FancyPackageName", Kind = LogicalLocationKind.Package },
                new LogicalLocation { ParentIndex = 0, Name = "my_method", FullyQualifiedName = @"FancyPackageName\my_method", Kind = LogicalLocationKind.Member }
            };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            ValidateLogicalLocations(expectedLogicalLocations, converter.LogicalLocations);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_GeneratesLocationWithOnlyPackage()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.Package = "FancyPackageName";
            builder.Module = null;
            builder.EntryPointName = null;

            var expectedLocation = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "FancyPackageName",
                    Index = 0
                }
            };

            var expectedLogicalLocations = new List<LogicalLocation>
            {
                new LogicalLocation {Kind = LogicalLocationKind.Package, FullyQualifiedName = "FancyPackageName" }
            };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            ValidateLogicalLocations(expectedLogicalLocations, converter.LogicalLocations);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_CanRecordSourceFileAndModule()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "File Goes Here";
            builder.Package = null;
            builder.Module = "LastResortModule";
            builder.EntryPointName = null;

            var expectedLocation = new Location
            {
                PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = new Uri("File Goes Here", UriKind.RelativeOrAbsolute)
                    },
                },
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "LastResortModule",
                    Index = 0
                }
            };

            var expectedLogicalLocations = new List<LogicalLocation>
            {
                new LogicalLocation { Kind = LogicalLocationKind.Module, FullyQualifiedName = "LastResortModule" }
            };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));
            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            ValidateLogicalLocations(expectedLogicalLocations, converter.LogicalLocations);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_RemovesProjectDirPrefix()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "file://$PROJECT_DIR$/mydir/myfile.xml";
            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);
            locationInfo.Location.PhysicalLocation.ArtifactLocation.Uri.ToString().Should().Be("mydir/myfile.xml");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_PersistsSourceLineInfo()
        {
            AndroidStudioProblem.Builder builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Line = 42;
            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);
            locationInfo.Location.PhysicalLocation.Region.StartLine.Should().Be(42);
        }

        private struct LocationInfo
        {
            public Location Location;
            public LogicalLocation LogicalLocation;
        }

        private static LocationInfo GetLocationInfoForBuilder(AndroidStudioProblem.Builder builder)
        {
            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            Location location = result.Locations.First();
            LogicalLocation logicalLocation =
                location.LogicalLocation.Index > -1
                ? converter.LogicalLocations[location.LogicalLocation.Index]
                : null;

            return new LocationInfo
            {
                Location = location,
                LogicalLocation = logicalLocation
            };
        }
    }
}
