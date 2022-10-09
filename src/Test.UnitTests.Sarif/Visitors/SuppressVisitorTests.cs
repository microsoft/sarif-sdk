// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

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
            var testCases = new[]
            {
                new
                {
                    Alias = string.Empty,
                    Justification = "some suppress justification",
                    Guids = false,
                    Timestamps = false,
                    ExpiryInDays = 0,
                    SuppressionStatus = SuppressionStatus.Accepted
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Guids = false,
                    Timestamps = false,
                    ExpiryInDays = 0,
                    SuppressionStatus = SuppressionStatus.Accepted
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Guids = true,
                    Timestamps = false,
                    ExpiryInDays = 0,
                    SuppressionStatus = SuppressionStatus.Accepted
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Guids = true,
                    Timestamps = true,
                    ExpiryInDays = 0,
                    SuppressionStatus = SuppressionStatus.Accepted
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Guids = true,
                    Timestamps = true,
                    ExpiryInDays = 1,
                    SuppressionStatus = SuppressionStatus.Accepted
                },
                new
                {
                    Alias = "some alias",
                    Justification = "some suppress justification",
                    Guids = true,
                    Timestamps = true,
                    ExpiryInDays = 1,
                    SuppressionStatus = SuppressionStatus.UnderReview
                },
            };

            foreach (var testCase in testCases)
            {
                VerifySuppressVisitor(testCase.Alias,
                                      testCase.Justification,
                                      testCase.Guids,
                                      testCase.Timestamps,
                                      testCase.ExpiryInDays,
                                      testCase.SuppressionStatus);
            }
        }

        private static void VerifySuppressVisitor(string alias,
                                                  string justification,
                                                  bool guids,
                                                  bool timestamps,
                                                  int expiryInDays,
                                                  SuppressionStatus suppressionStatus)
        {
            var visitor = new SuppressVisitor(justification,
                                              alias,
                                              guids,
                                              timestamps,
                                              expiryInDays,
                                              suppressionStatus);

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

                if (guids)
                {
                    suppression.Guid.Should().NotBeNull();
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
