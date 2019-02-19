// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FortifyConverterTests
    {
        [Fact]
        public void FortifyConverter_Convert_NullInput()
        {
            Assert.Throws<ArgumentNullException>(() => new FortifyConverter().Convert(null, null, OptionallyEmittedData.None));
        }

        [Fact]
        public void FortifyConverter_Convert_NullOutput()
        {
            using (var input = new MemoryStream())
            {
                Assert.Throws<ArgumentNullException>(() => new FortifyConverter().Convert(input, null, OptionallyEmittedData.None));
            }
        }

        private struct Builder
        {
            public string RuleId;
            public string InstanceId;
            public string Category;
            public string Kingdom;
            public string Abstract;
            public string AbstractCustom;
            public string Priority;
            public FortifyPathElement PrimaryOrSink;
            public FortifyPathElement Source;
            public ImmutableArray<int> CweIds;

            public FortifyIssue ToImmutable()
            {
                return new FortifyIssue
                    (
                        ruleId: this.RuleId,
                        iid: this.InstanceId,
                        category: this.Category,
                        kingdom: this.Kingdom,
                        abs: this.Abstract,
                        abstractCustom: this.AbstractCustom,
                        priority: this.Priority,
                        primaryOrSink: this.PrimaryOrSink,
                        source: this.Source,
                        cweIds: this.CweIds
                    );
            }
        }

        private static readonly FortifyPathElement s_dummyPathElement =
            new FortifyPathElement("filePath", 1729, "int Foo::Bar(string const&) const&&");
        private static readonly FortifyPathElement s_dummyPathSourceElement =
            new FortifyPathElement("sourceFilePath", 42, null);

        private static Builder GetBasicBuilder()
        {
            return new Builder
            {
                Category = "cat",
                Kingdom = "king",
                PrimaryOrSink = FortifyConverterTests.s_dummyPathElement
            };
        }

        private static FortifyIssue GetBasicIssue()
        {
            return FortifyConverterTests.GetBasicBuilder().ToImmutable();
        }

        [Fact]
        public void FortifyConverter_Convert_RuleIdIsKingdomAndCategory()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(FortifyConverterTests.GetBasicIssue());
            Assert.Equal("cat", result.RuleId);
        }

        [Fact]
        public void FortifyConverter_Convert_ToolFingerprintIsIid()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.InstanceId = "a";
            Result resultA = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.True(resultA.PartialFingerprints.Values.Contains("a"));

            builder.InstanceId = null; // IID is optional
            Result resultNull = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.Null(resultNull.PartialFingerprints);
        }

        [Fact]
        public void FortifyConverter_Convert_ShortMessageIsUnset()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(FortifyConverterTests.GetBasicIssue());
        }

        [Fact]
        public void FortifyConverter_Convert_FullMessageFallsBackToCategoryIfNoAbstractPresent()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(FortifyConverterTests.GetBasicIssue());
            result.Message.Text.Should().Contain("cat");
        }

        [Fact]
        public void FortifyConverter_Convert_FullMessageUsesAbstractIfPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.Abstract = "Some abstract message";
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.Equal("Some abstract message", result.Message.Text);
        }

        [Fact]
        public void FortifyConverter_Convert_FullMessageUsesAbstractCustomIfPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.AbstractCustom = "Some abstract custom message";
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.Equal("Some abstract custom message", result.Message.Text);
        }

        [Fact]
        public void FortifyConverter_Convert_ConcatenatesAbstractsIfBothPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.Abstract = "Some abstract message";
            builder.AbstractCustom = "Some abstract custom message";
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.Equal("Some abstract message" + Environment.NewLine + "Some abstract custom message",
                result.Message.Text);
        }

        [Fact]
        public void FortifyConverter_Convert_KingdomIsInProperties()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(FortifyConverterTests.GetBasicIssue());
            result.PropertyNames.Count.Should().Be(1);
            result.GetProperty("kingdom").Should().Be("king");
        }

        [Fact]
        public void FortifyConverter_Convert_FillsInPriorityIfFriorityPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.True(result.Properties == null || !result.PropertyNames.Contains("priority"),
                "Priority was set to a null value.");

            builder.Priority = "HIGH";
            result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.Equal("HIGH", result.GetProperty("priority"));
        }

        [Fact]
        public void FortifyConverter_Convert_FillsInCweIfPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.True(result.Properties == null || !result.Properties.ContainsKey("cwe"),
                "CWE was filled in when no CWEs were present.");

            builder.CweIds = ImmutableArray.Create(24, 42, 1729);
            result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.Equal("24, 42, 1729", result.GetProperty("cwe"));
        }

        [Fact]
        public void FortifyConverter_Convert_FillsInFortifyRuleIdIfPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.True(result.Properties == null || !result.PropertyNames.Contains("fortifyRuleId"),
                "Fortify RuleID was filled in when no ruleId was present.");

            builder.RuleId = "abc";
            result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.Equal("abc", result.GetProperty("fortifyRuleId"));
        }

        [Fact]
        public void FortifyConverter_Convert_UsesPrimaryAsMainLocation()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.Source = FortifyConverterTests.s_dummyPathSourceElement;
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.Equal(1, result.Locations.Count);
            Assert.Equal("filePath", result.Locations.First().PhysicalLocation.ArtifactLocation.Uri.ToString());
            Assert.True(result.Locations.First().PhysicalLocation.Region.ValueEquals(new Region { StartLine = 1729 }));
        }

        [Fact]
        public void FortifyConverter_Convert_DoesNotFillInCodeFlowWhenOnlyPrimaryIsPresent()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(GetBasicIssue());
            Assert.Null(result.CodeFlows);
        }

        [Fact]
        public void FortifyConverter_Convert_FillsInCodeFlowWhenSourceIsPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.Source = FortifyConverterTests.s_dummyPathSourceElement;
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.Equal(1, result.CodeFlows.Count);
            IList<ThreadFlowLocation> flowLocations = result.CodeFlows.First().ThreadFlows.First().Locations;
            Assert.Equal("sourceFilePath", flowLocations[0].Location.PhysicalLocation.ArtifactLocation.Uri.ToString());
            Assert.True(flowLocations[0].Location.PhysicalLocation.Region.ValueEquals(new Region { StartLine = 42 }));
            Assert.Equal("filePath", flowLocations[1].Location.PhysicalLocation.ArtifactLocation.Uri.ToString());
            Assert.True(flowLocations[1].Location.PhysicalLocation.Region.ValueEquals(new Region { StartLine = 1729 }));
        }
    }
}
