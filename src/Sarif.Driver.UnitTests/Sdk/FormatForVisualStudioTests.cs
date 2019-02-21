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
        private const string TestMessageStringId = "testMessageStringId";
        private const string TestAnalysisTarget = @"C:\dir\file";

        private static readonly ReportingDescriptor TestRule = new ReportingDescriptor
        {
            Id = TestRuleId,
            Name = new Message { Text = "ThisIsATest" },
            ShortDescription = new Message { Text = "short description" },
            FullDescription = new Message { Text = "full description" },
            MessageStrings = new Dictionary<string, MultiformatMessageString>
            {
                [TestMessageStringId] = new MultiformatMessageString { Text = "First: {0}, Second: {1}" }
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

        public static IEnumerable<object[]> FailureLevelFormatForVisualStudioTestCases => new[]
        {
            // Test each FailureLevel value.
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): error {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): error {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): error {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            new object[]
            {
                FailureLevel.Warning,
                ResultKind.Fail,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): warning {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            new object[]
            {
                FailureLevel.None,
                ResultKind.NotApplicable,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            new object[]
            {
                FailureLevel.Note,
                ResultKind.Fail,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            new object[]
            {
                FailureLevel.None,
                ResultKind.Pass,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            new object[]
            {
                FailureLevel.None,
                ResultKind.Open,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            new object[]
            {
                FailureLevel.None,
                ResultKind.Review,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            new object[]
            {
                FailureLevel.None,
                ResultKind.None,
                MultiLineTestRegion,
                $"{TestAnalysisTarget}(2,4,3,5): info {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            // Test formatting of a single-line multi-column region (previous tests used a multi-line region).
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                SingleLineMultiColumnTestRegion,
                $"{TestAnalysisTarget}(2,4-5): error {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            // Test formatting of a single-line single-column region.
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                SingleLineSingleColumnTestRegion,
                $"{TestAnalysisTarget}(2,4): error {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            // Test formatting of a single-line region with no column specified.
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                SingleLineNoColumnTestRegion,
                $"{TestAnalysisTarget}(2): error {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            // Test formatting of a multi-line region with no columns specified.
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                MultiLineNoColumnTestRegion,
                $"{TestAnalysisTarget}(2-3): error {TestRuleId}: First: 42, Second: 54",
                TestAnalysisTarget
            },

            // Test formatting of a relative path.
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                MultiLineNoColumnTestRegion,
                $"file(2-3): error {TestRuleId}: First: 42, Second: 54",
                "file"
            },

            // Test formatting of an absolute non-file URI
            new object[]
            {
                FailureLevel.Error,
                ResultKind.Fail,
                MultiLineNoColumnTestRegion,
                $"http://www.example.com/test.html(2-3): error {TestRuleId}: First: 42, Second: 54",
                "http://www.example.com/test.html"
            },
        };

        [Theory]
        [MemberData(nameof(FailureLevelFormatForVisualStudioTestCases))]
        public void Result_FormatFailureLevelsForVisualStudioTests(FailureLevel level, ResultKind kind, Region region, string expected, string path)
        {
            Result result = MakeResultFromTestCase(level, kind, region, path);

            string actual = result.FormatForVisualStudio(TestRule);

            actual.Should().Be(expected);
        }

private Result MakeResultFromTestCase(FailureLevel level, ResultKind kind, Region region, string path)
        {
            return new Result
            {
                RuleId = TestRuleId,
                Level = level,
                Kind = kind,
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(path, UriKind.RelativeOrAbsolute)
                            },
                            Region = region
                        }
                    }
                },
                Message = new Message
                {
                    Arguments = new List<string>
                    {
                        "42",
                        "54"
                    },
                    MessageId = TestMessageStringId
                }
            };
        }
    }
}
