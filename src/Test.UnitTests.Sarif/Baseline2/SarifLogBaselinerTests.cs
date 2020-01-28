// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Baseline
{
    public class SarifLogBaselinerTests
    {
        private struct ResultBaselineInformationTestCase
        {
            public string Name;
            public Result NewResult;
            public Result BaselineResult;
            public BaselineState ExpectedBaselineState;
            public DateTime ExpectedFirstDetectionTimeUtc;
        }

        private static readonly DateTime newRunTime = new DateTime(2020, 1, 28, 10, 9, 16, DateTimeKind.Utc);
        private static readonly DateTime baselineRunTime = new DateTime(2019, 12, 25, 7, 18, 25, DateTimeKind.Utc);
        private static readonly DateTime baselineResultTime = new DateTime(2019, 8, 3, 21, 0, 1, DateTimeKind.Utc);

        private static readonly List<ResultBaselineInformationTestCase> resultBaselineInformationTestCases =
            new List<ResultBaselineInformationTestCase>
            {
                new ResultBaselineInformationTestCase
                {
                    Name = "New",
                    NewResult = new Result(),
                    BaselineResult = null,
                    ExpectedBaselineState = BaselineState.New,
                    ExpectedFirstDetectionTimeUtc = newRunTime
                },

                new ResultBaselineInformationTestCase
                {
                    Name = "Existing",
                    NewResult = new Result(),
                    BaselineResult = new Result
                    {
                        Provenance = new ResultProvenance
                        {
                            FirstDetectionTimeUtc = baselineResultTime
                        }
                    },
                    ExpectedBaselineState = BaselineState.Unchanged,
                    ExpectedFirstDetectionTimeUtc = baselineResultTime
                },

                new ResultBaselineInformationTestCase
                {
                    Name = "Existing with provenance",
                    NewResult = new Result
                    {
                        Provenance = new ResultProvenance()
                    },
                    BaselineResult = new Result
                    {
                        Provenance = new ResultProvenance
                        {
                            FirstDetectionTimeUtc = baselineResultTime
                        }
                    },
                    ExpectedBaselineState = BaselineState.Unchanged,
                    ExpectedFirstDetectionTimeUtc = baselineResultTime
                },

                new ResultBaselineInformationTestCase
                {
                    Name = "Existing with no baseline provenance",
                    NewResult = new Result(),
                    BaselineResult = new Result(),
                    ExpectedBaselineState = BaselineState.Unchanged,
                    ExpectedFirstDetectionTimeUtc = baselineRunTime
                },

                new ResultBaselineInformationTestCase
                {
                    Name = "Existing with no baseline result time",
                    NewResult = new Result(),
                    BaselineResult = new Result
                    {
                        Provenance = new ResultProvenance()
                    },
                    ExpectedBaselineState = BaselineState.Unchanged,
                    ExpectedFirstDetectionTimeUtc = baselineRunTime
                },
            };

        [Fact]
        public void SarifLogBaseliner_CalculatesResultBaselineInformation()
        {
            var sb = new StringBuilder();

            foreach (ResultBaselineInformationTestCase testCase in resultBaselineInformationTestCases)
            {
                // Clone the result so we don't write over our test case data.
                Result newResult = testCase.NewResult.DeepClone();

                SarifLogBaseliner.SetResultBaselineInformation(newResult, testCase.BaselineResult, newRunTime, baselineRunTime);

                BaselineState actualBaselineState = newResult.BaselineState;
                DateTime? actualFirstDetectionTimeUtc = newResult?.Provenance?.FirstDetectionTimeUtc;

                if (actualBaselineState != testCase.ExpectedBaselineState ||
                    actualFirstDetectionTimeUtc != testCase.ExpectedFirstDetectionTimeUtc)
                {
                    sb.AppendLine($"    Test: {testCase.Name}");
                    sb.AppendLine($"        Expected: {testCase.ExpectedBaselineState}\t{testCase.ExpectedFirstDetectionTimeUtc}");
                    sb.AppendLine($"        Actual:   {actualBaselineState}\t{actualFirstDetectionTimeUtc}");
                }
            }

            sb.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{sb}");
        }

        private static readonly DateTime fakeNow = new DateTime(2020, 1, 28, 11, 9, 30, DateTimeKind.Utc);
        private static readonly DateTime runStartTime = new DateTime(2019, 5, 1, 0, 52, 42, DateTimeKind.Utc);
        private static readonly DateTime runEndTime = new DateTime(2019, 5, 1, 1, 2, 54, DateTimeKind.Utc);

        private struct RunTimeTestCase
        {
            public string Name;
            public Run Run;
            public DateTime ExpectedRunTime;
        }

        private static readonly List<RunTimeTestCase> runTimeTestCases = new List<RunTimeTestCase>
        {
            new RunTimeTestCase
            {
                Name = "No invocations",
                Run = new Run(),
                ExpectedRunTime = fakeNow
            },

            new RunTimeTestCase
            {
                Name = "Empty invocations",
                Run = new Run
                {
                    Invocations = new List<Invocation>()
                },
                ExpectedRunTime = fakeNow
            },

            new RunTimeTestCase
            {
                Name = "No start or end time",
                Run = new Run
                {
                    Invocations = new List<Invocation>
                    {
                        new Invocation()
                    }
                },
                ExpectedRunTime = fakeNow
            },

            new RunTimeTestCase
            {
                Name = "Start time only",
                Run = new Run
                {
                    Invocations = new List<Invocation>
                    {
                        new Invocation
                        {
                            StartTimeUtc = runStartTime
                        }
                    }
                },
                ExpectedRunTime = runStartTime
            },

            new RunTimeTestCase
            {
                Name = "End time only",
                Run = new Run
                {
                    Invocations = new List<Invocation>
                    {
                        new Invocation
                        {
                            EndTimeUtc = runEndTime
                        }
                    }
                },
                ExpectedRunTime = runEndTime
            },

            new RunTimeTestCase
            {
                Name = "Start and end times",
                Run = new Run
                {
                    Invocations = new List<Invocation>
                    {
                        new Invocation
                        {
                            StartTimeUtc = runStartTime,
                            EndTimeUtc = runEndTime
                        }
                    }
                },
                ExpectedRunTime = runEndTime
            },
        };

        [Fact]
        public void SarifLogBaseliner_CalculatesRunTime()
        {
            var sb = new StringBuilder();

            foreach (RunTimeTestCase testCase in runTimeTestCases)
            {
                DateTime actualRunTime = SarifLogBaseliner.GetRunTime(testCase.Run, fakeNow);
                if (actualRunTime != testCase.ExpectedRunTime)
                {
                    sb.AppendLine($"    Test: {testCase.Name}");
                    sb.AppendLine($"        Expected: {testCase.ExpectedRunTime}");
                    sb.AppendLine($"        Actual:   {actualRunTime}");
                }
            }

            sb.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{sb}");
        }
    }
}
