// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    // These tests test the extension method Result.FormatForVisualStudio.
    // But by providing various Region objects and ResultKind values, they
    // also exercise Region.FormatForVisualStudio and ResultKind.FormatForVisualStudio.
    public class FormatForVisualStudioTests
    {
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

        public static IEnumerable<object[]> FailureLevelFormatForVisualStudioTestCases => new[]
        {            
            // Default core failure cases, verbose and non-verbose
            BuildDefaultTestCase(FailureLevel.Error),
            BuildDefaultTestCase(FailureLevel.Warning),
            BuildDefaultTestCase(FailureLevel.Note),

            // Default non-failure cases (all of these are verbose only)
            BuildDefaultTestCase(FailureLevel.None, ResultKind.Pass),
            BuildDefaultTestCase(FailureLevel.None, ResultKind.Open),
            BuildDefaultTestCase(FailureLevel.None, ResultKind.Review),
            BuildDefaultTestCase(FailureLevel.None, ResultKind.NotApplicable),
            BuildDefaultTestCase(FailureLevel.None, ResultKind.Informational),

            // A special case, we treat the absence of either a kind or failure level as informational
            BuildDefaultTestCase(FailureLevel.None, ResultKind.None),

            // Special test cases for region variants
            // Test formatting of a single-line multi-column region (previous tests used a multi-line region).
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                SingleLineMultiColumnTestRegion,
                $"{TestData.TestAnalysisTarget}(2,4-5): error {TestData.TestRuleId}: First: 42, Second: 54",
                TestData.TestAnalysisTarget
            },

            // Test formatting of a single-line single-column region.
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                SingleLineSingleColumnTestRegion,
                $"{TestData.TestAnalysisTarget}(2,4): error {TestData.TestRuleId}: First: 42, Second: 54",
                TestData.TestAnalysisTarget
            },

            // Test formatting of a single-line region with no column specified.
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                SingleLineNoColumnTestRegion,
                $"{TestData.TestAnalysisTarget}(2): error {TestData.TestRuleId}: First: 42, Second: 54",
                TestData.TestAnalysisTarget
            },

            // Test formatting of a multi-line region with no columns specified.
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                MultiLineNoColumnTestRegion,
                $"{TestData.TestAnalysisTarget}(2-3): error {TestData.TestRuleId}: First: 42, Second: 54",
                TestData.TestAnalysisTarget
            },

            // Test formatting of a relative path.
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                MultiLineNoColumnTestRegion,
                $"file(2-3): error {TestData.TestRuleId}: First: 42, Second: 54",
                "file"
            },

            // Test formatting of an absolute non-file URI
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                MultiLineNoColumnTestRegion,
                $"http://www.example.com/test.html(2-3): error {TestData.TestRuleId}: First: 42, Second: 54",
                "http://www.example.com/test.html"
            },
        };

        private static object[] BuildDefaultTestCase(FailureLevel level, ResultKind kind = ResultKind.Fail)
        {
            string lineLabel = level != FailureLevel.None
                ? level.ToString().ToLowerInvariant()
                : kind.ToString().ToLowerInvariant();

            if (kind == ResultKind.Informational)
            {
                // Console reporting historically abbreviates this term
                lineLabel = "info";
            }

            if (level == FailureLevel.None && kind == ResultKind.None)
            {
                // No good information? Mark it as informational.
                lineLabel = "info";
            }

            return new object[]
            {
                level,
                kind,
                MultiLineTestRegion,
                $"{TestData.TestAnalysisTarget}(2,4,3,5): {lineLabel} {TestData.TestRuleId}: First: 42, Second: 54",
                TestData.TestAnalysisTarget
            };
        }

        [Theory]
        [MemberData(nameof(FailureLevelFormatForVisualStudioTestCases))]
        public void Result_FormatFailureLevelsForVisualStudioTests(FailureLevel level, ResultKind kind, Region region, string expected, string path)
        {
            Result result = TestData.CreateResult(level, kind, region, path);

            string actual = result.FormatForVisualStudio(TestData.TestRule);

            actual.Should().Be(expected);
        }
    }
}
