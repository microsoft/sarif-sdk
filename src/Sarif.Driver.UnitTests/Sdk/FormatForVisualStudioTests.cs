// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    // These tests test the extension method Result.FormatForVisualStudio.
    // But by providing various Region objects and ResultKind values, they
    // also exercise Region.FormatForVisualStudio and ResultKind.FormatForVisualStudio.
    public class FormatForVisualStudioTests
    {
        private const string TestRuleId = "TST0001";
        private const string TestFormatId = "testFormatSpecifier";
        private const string TestAnalysisTarget = @"C:\dir\file";

        private static readonly Rule TestRule = new Rule
        {
            Id = TestRuleId,
            Name = "ThisIsATest",
            ShortDescription = "short description",
            FullDescription = "full description",
            MessageFormats = new Dictionary<string, string>
            {
                [TestFormatId] = "First: {0}, Second: {1}"
            }
        };

        private static readonly Region MultiLineTestRegion = new Region
        {
            StartLine = 2,
            StartColumn = 4,
            EndLine = 3,
            EndColumn = 5
        };

        private static readonly Region SingleLineMultiColumnTestRegion = new Region
        {
            StartLine = 2,
            StartColumn = 4,
            EndLine = 2,
            EndColumn = 5
        };

        private static readonly Region SingleLineSingleColumnTestRegion = new Region
        {
            StartLine = 2,
            StartColumn = 4
        };

        private static readonly Region SingleLineNoColumnTestRegion = new Region
        {
            StartLine = 2
        };

        private static readonly Region MultiLineNoColumnTestRegion = new Region
        {
            StartLine = 2,
            EndLine = 3
        };

        public static IEnumerable<object[]> ResultFormatForVisualStudioTestCases => new[]
        {
            // Test each ResultKind value.
            new object[]
            {
                ResultLevel.Error,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): error {TestRuleId}: First: 42, Second: 54"
            },

            new object[]
            {
                ResultLevel.Error,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): error {TestRuleId}: First: 42, Second: 54"
            },

            new object[]
            {
                ResultLevel.Error,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): error {TestRuleId}: First: 42, Second: 54"
            },

            new object[]
            {
                ResultLevel.Warning,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): warning {TestRuleId}: First: 42, Second: 54"
            },

            new object[]
            {
                ResultLevel.NotApplicable,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54"
            },

            new object[]
            {
                ResultLevel.Note,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54"
            },

            new object[]
            {
                ResultLevel.Pass,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54"
            },

            new object[]
            {
                ResultLevel.Unknown,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54"
            },

            // Test formatting of a single-line multi-column region (previous tests used a multi-line region).
            new object[]
            {
                ResultLevel.Error,
                SingleLineMultiColumnTestRegion,
                $"{TestAnalysisTarget}(2,4-5): error {TestRuleId}: First: 42, Second: 54"
            },

            // Test formatting of a single-line single-column region.
            new object[]
            {
                ResultLevel.Error,
                SingleLineSingleColumnTestRegion,
                $"{TestAnalysisTarget}(2,4): error {TestRuleId}: First: 42, Second: 54"
            },

            // Test formatting of a single-line region with no column specified.
            new object[]
            {
                ResultLevel.Error,
                SingleLineNoColumnTestRegion,
                $"{TestAnalysisTarget}(2): error {TestRuleId}: First: 42, Second: 54"
            },

            // Test formatting of a multi-line region with no columns specified.
            new object[]
            {
                ResultLevel.Error,
                MultiLineNoColumnTestRegion,
                $"{TestAnalysisTarget}(2-3): error {TestRuleId}: First: 42, Second: 54"
            },
        };

        [Theory]
        [MemberData(nameof(ResultFormatForVisualStudioTestCases))]
        public void Result_FormatForVisualStudioTests(ResultLevel level, Region region, string expected)
        {
            Result result = MakeResultFromTestCase(level, region);

            string actual = result.FormatForVisualStudio(TestRule);

            actual.Should().Be(expected);
        }

        private Result MakeResultFromTestCase(ResultLevel level, Region region)
        {
            return new Result
            {
                RuleId = TestRuleId,
                Level = level,
                Locations = new List<Location>
                {
                    new Location
                    {
                        AnalysisTarget = new PhysicalLocation
                        {
                            Uri = new Uri(TestAnalysisTarget),
                            Region = region
                        }
                    }
                },
                FormattedRuleMessage = new FormattedRuleMessage
                {
                    FormatId = TestFormatId,
                    Arguments = new List<string>
                    {
                        "42",
                        "54"
                    }
                }
            };
        }
    }
}
