// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Visitors;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Visitors
{
    public class SuppressVisitorTests
    {
        private const int DateTimeAssertPrecision = 500;

        [Fact]
        public void SuppressVisitor_ShouldFlowPropertiesCorrectly()
        {
            var guids = new List<string>() { "704cf481-0cfd-46ae-90cd-533cdc6c3bb4", "ecaa7988-5cef-411b-b468-6c20851d6994", "c65b76c7-3cd6-4381-9216-430bcc7fab2d", "04753e26-d297-43e2-a7f7-ae2d34c398c9", "54cb1f58-f401-4f8e-8f42-f2482a123b85" };
            var testCases = new[]
            {
                new
                {
                    Alias = string.Empty,
                    Justification = "some suppress justification",
                    Uuids = false,
                    Timestamps = false,
                    ExpiryInDays = 0,
                    SuppressionStatus = SuppressionStatus.Accepted,
                    Guids = new List<string>()
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Uuids = false,
                    Timestamps = false,
                    ExpiryInDays = 0,
                    SuppressionStatus = SuppressionStatus.Accepted,
                    Guids = new List<string>()
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Uuids = true,
                    Timestamps = false,
                    ExpiryInDays = 0,
                    SuppressionStatus = SuppressionStatus.Accepted,
                    Guids = new List<string>()
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Uuids = true,
                    Timestamps = true,
                    ExpiryInDays = 0,
                    SuppressionStatus = SuppressionStatus.Accepted,
                    Guids = new List<string>()
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Uuids = true,
                    Timestamps = true,
                    ExpiryInDays = 1,
                    SuppressionStatus = SuppressionStatus.Accepted,
                    Guids = new List<string>()
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Uuids = true,
                    Timestamps = true,
                    ExpiryInDays = 1,
                    SuppressionStatus = SuppressionStatus.UnderReview,
                    Guids = new List<string>()
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Uuids = true,
                    Timestamps = true,
                    ExpiryInDays = 1,
                    SuppressionStatus = SuppressionStatus.Accepted,
                    Guids = guids.ToList()
                },
            };

            foreach (var testCase in testCases)
            {
                VerifySuppressVisitor(testCase.Alias,
                                      testCase.Justification,
                                      testCase.Uuids,
                                      testCase.Timestamps,
                                      testCase.ExpiryInDays,
                                      testCase.SuppressionStatus,
                                      testCase.Guids);
            }
        }

        private static void VerifySuppressVisitor(string alias,
                                                  string justification,
                                                  bool uuids,
                                                  bool timestamps,
                                                  int expiryInDays,
                                                  SuppressionStatus suppressionStatus,
                                                  IEnumerable<string> guids)
        {
            var visitor = new SuppressVisitor(justification,
                                              alias,
                                              uuids,
                                              timestamps,
                                              expiryInDays,
                                              suppressionStatus, 
                                              guids);

            var random = new Random();
            SarifLog current = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, runCount: 1, resultCount: 1);
            SarifLog suppressed = visitor.VisitSarifLog(current);
            IList<Result> results = suppressed.Runs[0].Results;
            foreach (Result result in results)
            {
                result.Suppressions.Should().NotBeNullOrEmpty();

                Suppression suppression = result.Suppressions[0];
                suppression.Status.Should().Be(suppressionStatus);
                suppression.Justification.Should().Be(justification);
                suppression.Kind.Should().Be(SuppressionKind.External);

                if (!string.IsNullOrWhiteSpace(alias))
                {
                    suppression.GetProperty("alias").Should().Be(alias);
                }

                if (uuids)
                {
                    suppression.Guid.Should().NotBeNullOrEmpty();
                }

                if (timestamps && suppression.TryGetProperty("timeUtc", out DateTime timeUtc))
                {
                    timeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeAssertPrecision);
                }

                if (expiryInDays > 0 && suppression.TryGetProperty("expiryUtc", out DateTime expiryUtc))
                {
                    expiryUtc.Should().BeCloseTo(DateTime.UtcNow.AddDays(expiryInDays), DateTimeAssertPrecision);
                }
            }
        }
    }
}
