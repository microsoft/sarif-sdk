﻿// Copyright (c) Microsoft. All rights reserved.
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
    public class AndroidStudioConverterTests
    {
        private AndroidStudioConverter _converter = null;

        public AndroidStudioConverterTests()
        {
            _converter = new AndroidStudioConverter();
        }

        [Fact]
        public void AndroidStudioConverter_Convert_Nulls()
        {
            Assert.Throws<ArgumentNullException>(() => _converter.Convert(null, null, LoggingOptions.None));
        }

        [Fact]
        public void AndroidStudioConverter_Convert_NullInput()
        {
            Assert.Throws<ArgumentNullException>(() => _converter.Convert(null, new ResultLogObjectWriter(), LoggingOptions.None));
        }

        [Fact]
        public void AndroidStudioConverter_Convert_NullOutput()
        {
            Assert.Throws<ArgumentNullException>(() => _converter.Convert(new MemoryStream(), null, LoggingOptions.None));
        }

        private const string EmptyResult = @"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""AndroidStudio""
      },
      ""results"": []
    }
  ]
}";
        [Fact]
        public void AndroidStudioConverter_Convert_NoResults()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            actualJson.Should().BeCrossPlatformEquivalent(EmptyResult);
        }

        [Fact]
        public void AndroidStudioConverter_Convert_EmptyResult()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem></problem></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            actualJson.Should().BeCrossPlatformEquivalent(EmptyResult);
        }

        [Fact]
        public void AndroidStudioConverter_Convert_EmptyResultSingleTag()
        {
            string androidStudioLog = @"<?xml version=""1.0"" encoding=""UTF-8""?><problems><problem /></problems>";
            string actualJson = Utilities.GetConverterJson(_converter, androidStudioLog);
            actualJson.Should().BeCrossPlatformEquivalent(EmptyResult);
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
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Description = "Cute fluffy kittens";
            var uut = new AndroidStudioProblem(builder);
            string result = AndroidStudioConverter.GetShortDescriptionForProblem(uut);
            Assert.Equal("Cute fluffy kittens", result);
        }

        [Fact]
        public void AndroidStudioConverter_GetShortDescription_UsesProblemClassIfDescriptionNotPresent()
        {
            var uut = AndroidStudioProblemTests.GetDefaultProblem();
            string result = AndroidStudioConverter.GetShortDescriptionForProblem(uut);
            result.Should().Contain("A Problematic Problem");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_EmptyHintsDoNotAffectDescription()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
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
            var uut = AndroidStudioProblemTests.GetDefaultProblem();
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            Assert.Equal("A Problematic Problem", result.RuleId);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_HasNoPropertiesIfAttributeKeyAndSeverity()
        {
            var uut = AndroidStudioProblemTests.GetDefaultProblem();
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            Assert.Null(result.Properties);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_AttributeKeyIsPersistedInProperties()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.AttributeKey = "key";
            var uut = new AndroidStudioProblem(builder);
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            result.PropertyNames.Count.Should().Be(1);
            result.GetProperty("attributeKey").Should().Be("key");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_SeverityIsPersistedInProperties()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.Severity = "warning";
            var uut = new AndroidStudioProblem(builder);
            Result result = new AndroidStudioConverter().ConvertProblemToSarifResult(uut);
            result.PropertyNames.Count.Should().Be(1);
            result.GetProperty("severity").Should().Be("warning");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertToSarifResult_MultiplePropertiesArePersisted()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
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
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "expected_file.java";
            builder.EntryPointType = "file";
            builder.EntryPointName = "bad_file.java";
            Location loc = GetLocationInfoForBuilder(builder).Location;
            loc.PhysicalLocation.FileLocation.Uri.ToString().Should().Be("expected_file.java");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_DoesNotRecordTopLevelEntryPointAsSourceFile()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = null;
            builder.EntryPointType = "file";
            builder.EntryPointName = "expected_file.java";
            Location loc = GetLocationInfoForBuilder(builder).Location;
            loc.PhysicalLocation.Should().BeNull();
        }

        [Fact]
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

            var expectedLogicalLocations = new Dictionary<string, LogicalLocation>
            {
                {
                    "my_fancy_binary", new LogicalLocation { ParentKey = null, Name = "my_fancy_binary", Kind = LogicalLocationKind.Module }
                },
                {
                    @"my_fancy_binary\my_method",
                    new LogicalLocation { ParentKey = "my_fancy_binary", Name = "my_method", Kind = LogicalLocationKind.Member }
                },
           };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            foreach (string key in expectedLogicalLocations.Keys)
            {
                expectedLogicalLocations[key].ValueEquals(converter.LogicalLocationsDictionary[key]).Should().BeTrue();
            }
            converter.LogicalLocationsDictionary.Count.Should().Be(expectedLogicalLocations.Count);
        }

        [Fact]
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

            var expectedLogicalLocations = new Dictionary<string, LogicalLocation>
            {
                {
                    "my_method", new LogicalLocation { ParentKey = null, Name = "my_method", Kind = LogicalLocationKind.Member }
                },
           };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            foreach (string key in expectedLogicalLocations.Keys)
            {
                expectedLogicalLocations[key].ValueEquals(converter.LogicalLocationsDictionary[key]).Should().BeTrue();
            }
            converter.LogicalLocationsDictionary.Count.Should().Be(expectedLogicalLocations.Count);
        }

        [Fact]
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
            
            var expectedLogicalLocations = new Dictionary<string, LogicalLocation>
            {
                {
                    "FancyPackageName", new LogicalLocation { ParentKey = null, Name = "FancyPackageName", Kind = LogicalLocationKind.Package }
                },
                {
                    @"FancyPackageName\my_method", new LogicalLocation { ParentKey = "FancyPackageName", Name = "my_method", Kind = LogicalLocationKind.Member }
                },
           };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            foreach (string key in expectedLogicalLocations.Keys)
            {
                expectedLogicalLocations[key].ValueEquals(converter.LogicalLocationsDictionary[key]).Should().BeTrue();
            }
            converter.LogicalLocationsDictionary.Count.Should().Be(expectedLogicalLocations.Count);
        }

        [Fact]
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

            var expectedLogicalLocations = new Dictionary<string, LogicalLocation>
            {
                {
                    "FancyPackageName", new LogicalLocation { ParentKey = null, Name = "FancyPackageName", Kind = LogicalLocationKind.Package }
                }
           };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            foreach (string key in expectedLogicalLocations.Keys)
            {
                expectedLogicalLocations[key].ValueEquals(converter.LogicalLocationsDictionary[key]).Should().BeTrue();
            }
            converter.LogicalLocationsDictionary.Count.Should().Be(expectedLogicalLocations.Count);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_CanRecordSourceFileAndModule()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "File Goes Here";
            builder.Package = null;
            builder.Module = "LastResortModule";
            builder.EntryPointName = null;

            var expectedLocation = new Location
            {
                PhysicalLocation = new PhysicalLocation
                {
                    FileLocation = new FileLocation
                    {
                        Uri = new Uri("File Goes Here", UriKind.RelativeOrAbsolute)
                    },
                },
                FullyQualifiedLogicalName = "LastResortModule"
            };

            var expectedLogicalLocations = new Dictionary<string, LogicalLocation>
            {
                {
                    "LastResortModule", new LogicalLocation { ParentKey = null, Name = "LastResortModule", Kind = LogicalLocationKind.Module }
                }
           };

            var converter = new AndroidStudioConverter();
            Result result = converter.ConvertProblemToSarifResult(new AndroidStudioProblem(builder));

            result.Locations[0].ValueEquals(expectedLocation).Should().BeTrue();

            foreach (string key in expectedLogicalLocations.Keys)
            {
                expectedLogicalLocations[key].ValueEquals(converter.LogicalLocationsDictionary[key]).Should().BeTrue();
            }
            converter.LogicalLocationsDictionary.Count.Should().Be(expectedLogicalLocations.Count);
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_RemovesProjectDirPrefix()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
            builder.File = "file://$PROJECT_DIR$/mydir/myfile.xml";
            LocationInfo locationInfo = GetLocationInfoForBuilder(builder);
            locationInfo.Location.PhysicalLocation.FileLocation.Uri.ToString().Should().Be("mydir/myfile.xml");
        }

        [Fact]
        public void AndroidStudioConverter_ConvertSarifResult_PersistsSourceLineInfo()
        {
            var builder = AndroidStudioProblemTests.GetDefaultProblemBuilder();
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

            string logicalLocationKey = converter.LogicalLocationsDictionary.Keys.SingleOrDefault();
            LogicalLocation logicalLocation = logicalLocationKey != null
                ? converter.LogicalLocationsDictionary[logicalLocationKey]
                : null;

            return new LocationInfo
            {
                Location = location,
                LogicalLocation = logicalLocation
            };
        }
    }
}
