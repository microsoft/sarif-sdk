// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Baseline2
{
    public class MatchedResultsTests
    {
        private static readonly DateTime previousRunStartTime = DateTime.Parse("2019-01-28T15:03:40Z");
        private static readonly DateTime previousRunEndTime = DateTime.Parse("2019-01-28T15:12:05Z");
        private static readonly DateTime currentRunStartTime = DateTime.Parse("2020-01-28T15:03:40Z");
        private static readonly DateTime currentRunEndTime = DateTime.Parse("2020-01-28T15:12:05Z");
        private static readonly DateTime firstDetectionTime = DateTime.Parse("2020-01-28T15:03:40Z");

        // If there is no detection time information available, the baseliner chooses the current time.
        // We don't have an "IDateTimeService" to inject as a dependency, so just save the current time
        // and assert that in all such cases, the first detection time is at least this value:
        private static readonly DateTime fakeNow = DateTime.UtcNow;

        private struct FirstDetectionTimeTestCase
        {
            public string Name;
            public Result PreviousResult;
            public Run PreviousRun;
            public Result CurrentResult;
            public Run CurrentRun;
            public BaselineState ExpectedBaselineState;
            public DateTime? ExpectedFirstDetectionTime;
        }

        private static readonly List<FirstDetectionTimeTestCase> firstDetectionTimeTestCases =
            new List<FirstDetectionTimeTestCase>
            {
                new FirstDetectionTimeTestCase
                {
                    Name = "New, no time information",
                    PreviousResult = null,
                    PreviousRun = new Run(),
                    CurrentResult = new Result(),
                    CurrentRun = new Run(),
                    ExpectedBaselineState = BaselineState.New,
                    ExpectedFirstDetectionTime = null   // Means "expect the current time".
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "New, provenance, no time information",
                    PreviousResult = null,
                    PreviousRun = new Run(),
                    CurrentResult = new Result
                    {
                        Provenance = new ResultProvenance()
                    },
                    CurrentRun = new Run(),
                    ExpectedBaselineState = BaselineState.New,
                    ExpectedFirstDetectionTime = null
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "New, provenance with time information",
                    PreviousResult = null,
                    PreviousRun = new Run(),
                    CurrentResult = new Result
                    {
                        Provenance = new ResultProvenance
                        {
                            FirstDetectionTimeUtc = firstDetectionTime
                        }
                    },
                    CurrentRun = new Run(),
                    ExpectedBaselineState = BaselineState.New,
                    ExpectedFirstDetectionTime = firstDetectionTime
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "New, current run end time only",
                    PreviousResult = null,
                    PreviousRun = new Run(),
                    CurrentResult = new Result(),
                    CurrentRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                EndTimeUtc = currentRunEndTime
                            }
                        }
                    },
                    ExpectedBaselineState = BaselineState.New,
                    ExpectedFirstDetectionTime = currentRunEndTime
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "New, current run start time only",
                    PreviousResult = null,
                    PreviousRun = new Run(),
                    CurrentResult = new Result(),
                    CurrentRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                StartTimeUtc = currentRunStartTime
                            }
                        }
                    },
                    ExpectedBaselineState = BaselineState.New,
                    ExpectedFirstDetectionTime = currentRunStartTime
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "New, current start and end times",
                    PreviousResult = null,
                    PreviousRun = new Run(),
                    CurrentResult = new Result(),
                    CurrentRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                StartTimeUtc = currentRunStartTime,
                                EndTimeUtc = currentRunEndTime
                            }
                        }
                    },
                    ExpectedBaselineState = BaselineState.New,
                    ExpectedFirstDetectionTime = currentRunEndTime
                },
                new FirstDetectionTimeTestCase // TODO TEST CASE SHOWING CURRENT RUN WINS FOR NEW AND PREV RUN WINS FOR EXISTING
                {
                    Name = "New, current run wins",
                    PreviousResult = null,
                    PreviousRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                EndTimeUtc = previousRunEndTime
                            }
                        }
                    },
                    CurrentResult = new Result(),
                    CurrentRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                EndTimeUtc = currentRunEndTime
                            }
                        }
                    },
                    ExpectedBaselineState = BaselineState.New,
                    ExpectedFirstDetectionTime = currentRunEndTime
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "Absent, no time information",
                    PreviousResult = new Result(),
                    PreviousRun = new Run(),
                    CurrentResult = null,
                    CurrentRun = new Run(),
                    ExpectedBaselineState = BaselineState.Absent,
                    ExpectedFirstDetectionTime = null
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "Absent, time from previous run",
                    PreviousResult = new Result(),
                    PreviousRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                EndTimeUtc = previousRunEndTime
                            }
                        }
                    },
                    CurrentResult = null,
                    CurrentRun = new Run(),
                    ExpectedBaselineState = BaselineState.Absent,
                    ExpectedFirstDetectionTime = previousRunEndTime
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "Absent, time from previous result",
                    PreviousResult = new Result
                    {
                        Provenance = new ResultProvenance
                        {
                            FirstDetectionTimeUtc = firstDetectionTime // Should override the information from the run.
                        }
                    },
                    PreviousRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                EndTimeUtc = previousRunEndTime
                            }
                        }
                    },
                    CurrentResult = null,
                    CurrentRun = new Run(),
                    ExpectedBaselineState = BaselineState.Absent,
                    ExpectedFirstDetectionTime = firstDetectionTime
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "Unchanged, time from previous result", // No need to repeat all the "time selection" cases from tests above for the "Unchanged" case.
                    PreviousResult = new Result
                    {
                        Provenance = new ResultProvenance
                        {
                            FirstDetectionTimeUtc = firstDetectionTime
                        }
                    },
                    PreviousRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                EndTimeUtc = previousRunEndTime
                            }
                        }
                    },
                    CurrentResult = new Result(), // Matches because the result matcher looks at only certain properties, and Provenance isn't one of them.
                    CurrentRun = new Run(),
                    ExpectedBaselineState = BaselineState.Unchanged,
                    ExpectedFirstDetectionTime = firstDetectionTime
                },
                new FirstDetectionTimeTestCase
                {
                    Name = "Existing, previous run wins",
                    PreviousResult = new Result(),
                    PreviousRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                EndTimeUtc = previousRunEndTime
                            }
                        }
                    },
                    CurrentResult = new Result(),
                    CurrentRun = new Run
                    {
                        Invocations = new List<Invocation>
                        {
                            new Invocation
                            {
                                EndTimeUtc = currentRunEndTime
                            }
                        }
                    },
                    ExpectedBaselineState = BaselineState.Unchanged,
                    ExpectedFirstDetectionTime = previousRunEndTime
                }
            };

        [Fact]
        public void MatchedResults_SetsFirstDetectionTime()
        {
            var sb = new StringBuilder();

            foreach (FirstDetectionTimeTestCase testCase in firstDetectionTimeTestCases)
            {
                DateTime expectedFirstDetectionTime = testCase.ExpectedFirstDetectionTime.HasValue
                    ? testCase.ExpectedFirstDetectionTime.Value
                    : fakeNow;

                ExtractedResult previousExtracted = testCase.PreviousResult != null
                    ? new ExtractedResult(testCase.PreviousResult, testCase.PreviousRun)
                    : null;
                ExtractedResult currentExtracted = testCase.CurrentResult != null
                    ? new ExtractedResult(testCase.CurrentResult, testCase.CurrentRun)
                    : null;
                MatchedResults matchedResults = new MatchedResults(previousExtracted, currentExtracted);

                Result result = matchedResults.CalculateBasedlinedResult(DictionaryMergeBehavior.InitializeFromMostRecent /* arbitrary */);

                BaselineState actualBaselineState = result.BaselineState;
                DateTime? actualFirstDetectionTime = result?.Provenance?.FirstDetectionTimeUtc;
                if (actualFirstDetectionTime.HasValue && actualFirstDetectionTime.Value >= fakeNow)
                {
                    actualFirstDetectionTime = fakeNow;
                }

                if (actualBaselineState != testCase.ExpectedBaselineState ||
                    actualFirstDetectionTime != expectedFirstDetectionTime)
                {
                    sb.AppendLine($"    Test: {testCase.Name}");
                    sb.AppendLine($"        Expected: {testCase.ExpectedBaselineState}\t{testCase.ExpectedFirstDetectionTime}");
                    sb.AppendLine($"        Actual:   {actualBaselineState}\t{actualFirstDetectionTime}");
                }
            }

            sb.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{sb}");
        }
    }
}
