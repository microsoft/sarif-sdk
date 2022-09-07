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
        public void SuppressVisitor_ShouldFlowPropertiesCorrectly_EmptyAlias()
        {
            var testCase = new
            {
                alias = string.Empty,
                justification = "some suppress justification",
                uuids = false,
                timestamps = false,
                expiryInDays = 0,
                suppressionStatus = SuppressionStatus.Accepted,
                resultGuids = default(List<string>),
            };

            VerifySuppressVisitor(testCase.alias,
                                  testCase.justification,
                                  testCase.uuids,
                                  testCase.timestamps,
                                  testCase.expiryInDays,
                                  testCase.suppressionStatus,
                                  testCase.resultGuids);
        }

        [Fact]
        public void SuppressVisitor_ShouldFlowPropertiesCorrectly_With_Alias()
        {
            var testCase = new
            {
                alias = "some alias",
                justification = "some suppress justification",
                uuids = false,
                timestamps = false,
                expiryInDays = 0,
                suppressionStatus = SuppressionStatus.Accepted,
                resultGuids = default(List<string>),
            };

            VerifySuppressVisitor(testCase.alias,
                                  testCase.justification,
                                  testCase.uuids,
                                  testCase.timestamps,
                                  testCase.expiryInDays,
                                  testCase.suppressionStatus,
                                  testCase.resultGuids);
        }

        [Fact]
        public void SuppressVisitor_ShouldFlowPropertiesCorrectly_With_Uuids()
        {
            var testCase = new
            {
                alias = "some alias",
                justification = "some suppress justification",
                uuids = true,
                timestamps = false,
                expiryInDays = 0,
                suppressionStatus = SuppressionStatus.Accepted,
                resultGuids = default(List<string>),
            };

            VerifySuppressVisitor(testCase.alias,
                                  testCase.justification,
                                  testCase.uuids,
                                  testCase.timestamps,
                                  testCase.expiryInDays,
                                  testCase.suppressionStatus,
                                  testCase.resultGuids);
        }

        [Fact]
        public void SuppressVisitor_ShouldFlowPropertiesCorrectly_With_Timestamps()
        {
            var testCase = new
            {
                alias = "some alias",
                justification = "some suppress justification",
                uuids = true,
                timestamps = true,
                expiryInDays = 0,
                suppressionStatus = SuppressionStatus.Accepted,
                resultGuids = default(List<string>),
            };

            VerifySuppressVisitor(testCase.alias,
                                  testCase.justification,
                                  testCase.uuids,
                                  testCase.timestamps,
                                  testCase.expiryInDays,
                                  testCase.suppressionStatus,
                                  testCase.resultGuids);
        }

        [Fact]
        public void SuppressVisitor_ShouldFlowPropertiesCorrectly_With_Expiry()
        {
            var testCase = new
            {
                alias = "some alias",
                justification = "some suppress justification",
                uuids = true,
                timestamps = true,
                expiryInDays = 1,
                suppressionStatus = SuppressionStatus.Accepted,
                resultGuids = default(List<string>),
            };

            VerifySuppressVisitor(testCase.alias,
                                  testCase.justification,
                                  testCase.uuids,
                                  testCase.timestamps,
                                  testCase.expiryInDays,
                                  testCase.suppressionStatus,
                                  testCase.resultGuids);
        }

        [Fact]
        public void SuppressVisitor_ShouldFlowPropertiesCorrectly_UnderReview()
        {
            var testCase = new
            {
                alias = "some alias",
                justification = "some suppress justification",
                uuids = true,
                timestamps = true,
                expiryInDays = 1,
                suppressionStatus = SuppressionStatus.UnderReview,
                resultGuids = default(List<string>),
            };

            VerifySuppressVisitor(testCase.alias,
                                  testCase.justification,
                                  testCase.uuids,
                                  testCase.timestamps,
                                  testCase.expiryInDays,
                                  testCase.suppressionStatus,
                                  testCase.resultGuids);
        }

        [Fact]
        public void SuppressVisitor_ShouldFlowPropertiesCorrectly_With_Result_Guids()
        {
            var testCase = new
            {
                alias = "some alias",
                justification = "some suppress justification",
                uuids = true,
                timestamps = true,
                expiryInDays = 1,
                suppressionStatus = SuppressionStatus.Accepted,
                resultGuids = new List<string>() { "704cf481-0cfd-46ae-90cd-533cdc6c3bb4", "ecaa7988-5cef-411b-b468-6c20851d6994", "c65b76c7-3cd6-4381-9216-430bcc7fab2d", "04753e26-d297-43e2-a7f7-ae2d34c398c9", "54cb1f58-f401-4f8e-8f42-f2482a123b85" },
            };

            VerifySuppressVisitor(testCase.alias,
                                  testCase.justification,
                                  testCase.uuids,
                                  testCase.timestamps,
                                  testCase.expiryInDays,
                                  testCase.suppressionStatus,
                                  testCase.resultGuids);
        }

        [Fact]
        public void SuppressVisitor_ShouldFlowPropertiesCorrectly_Empty_Result_Guids()
        {
            var testCase = new
            {
                alias = "some alias",
                justification = "some suppress justification",
                uuids = true,
                timestamps = true,
                expiryInDays = 1,
                suppressionStatus = SuppressionStatus.Accepted,
                resultGuids = new List<string>() { },
            };

            VerifySuppressVisitor(testCase.alias,
                                  testCase.justification,
                                  testCase.uuids,
                                  testCase.timestamps,
                                  testCase.expiryInDays,
                                  testCase.suppressionStatus,
                                  testCase.resultGuids);
        }

        [Fact]
        public void SuppressVisitor_ShouldNotDuplicateEntries()
        {
            var testCase = new
            {
                alias = "some alias",
                justification = "some suppress justification",
                uuids = true,
                timestamps = false,
                expiryInDays = 0,
                suppressionStatus = SuppressionStatus.Accepted,
                resultGuids = default(List<string>),
            };

            var random = new Random();
            SarifLog current = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, runCount: 1, resultCount: 1);

            var visitor = new SuppressVisitor(testCase.justification,
                                              testCase.alias,
                                              testCase.uuids,
                                              testCase.timestamps,
                                              testCase.expiryInDays,
                                              testCase.suppressionStatus,
                                              testCase.resultGuids);

            SarifLog suppressed = visitor.VisitSarifLog(current);
            IList<Result> results = suppressed.Runs[0].Results;

            // Verify suppression was added
            foreach (Result result in results)
            {
                result.Suppressions.Should().NotBeNullOrEmpty();
                result.Suppressions.Count.Should().Be(1);
            }

            // Suppress a second time
            SarifLog suppressed2 = visitor.VisitSarifLog(suppressed);

            // Verify duplicate suppression was not added
            foreach (Result result in results)
            {
                result.Suppressions.Should().NotBeNullOrEmpty();
                result.Suppressions.Count.Should().Be(1);
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
                //Suppressions will not exist if guids is an empty search
                if (guids != null && !guids.Any())
                {
                    result.Suppressions.Should().BeNullOrEmpty();
                    return;
                }

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

                if (guids != null)
                {
                    suppression.Should().Match(b => (guids.Contains(result.Guid, StringComparer.OrdinalIgnoreCase)));
                }
            }
        }
    }
}
