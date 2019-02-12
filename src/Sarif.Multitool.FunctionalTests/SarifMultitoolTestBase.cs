// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public abstract class SarifMultitoolTestBase
    {
        protected const string JsonSchemaFile = "sarif-schema.json";
        protected const string TestDataDirectory = "TestData";

        protected static string MakeExpectedFilePath(string testDirectory, string testFileName)
        {
            return MakeQualifiedFilePath(testDirectory, testFileName, "Expected");
        }

        protected static string MakeActualFilePath(string testDirectory, string testFileName)
        {
            return MakeQualifiedFilePath(testDirectory, testFileName, "Actual");
        }

        // We can't just compare the text of the log files because properties
        // like start time, and absolute paths, will differ from run to run.
        // Until SarifLogger has a "deterministic" option (see http://github.com/Microsoft/sarif-sdk/issues/500),
        // we perform a selective compare of just the elements we care about.
        protected static void SelectiveCompare(string actualLogContents, string expectedLogContents)
        {
            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(actualLogContents);
            SarifLog expectedLog = JsonConvert.DeserializeObject<SarifLog>(expectedLogContents);

            SelectiveCompare(actualLog, expectedLog);
        }

        private static string MakeQualifiedFilePath(string testDirectory, string testFileName, string qualifier)
        {
            string qualifiedFileName =
                Path.GetFileNameWithoutExtension(testFileName) + "_" + qualifier + Path.GetExtension(testFileName);

            return Path.Combine(testDirectory, qualifiedFileName);
        }

        private static void SelectiveCompare(SarifLog actualLog, SarifLog expectedLog)
        {
            IList<Result> actualResults = actualLog.Runs[0].Results;
            IList<Result> expectedResults = expectedLog.Runs[0].Results;

            SelectiveCompare(actualResults, expectedResults);

            IList<Notification> actualConfigurationNotifications = actualLog.Runs[0].Invocations?[0]?.ConfigurationNotifications;
            IList<Notification> expectedConfigurationNotifications = expectedLog.Runs[0].Invocations?[0]?.ConfigurationNotifications;

            SelectiveCompare(actualConfigurationNotifications, expectedConfigurationNotifications);

            IList<Notification> actualToolNotifications = actualLog.Runs[0].Invocations?[0]?.ToolNotifications;
            IList<Notification> expectedToolNotifications = expectedLog.Runs[0].Invocations[0]?.ToolNotifications;

            SelectiveCompare(actualToolNotifications, expectedToolNotifications);

            IList<MessageDescriptor> actualRules = actualLog.Runs[0].Tool.Driver.RulesMetadata;
            IList<MessageDescriptor> expectedRules = expectedLog.Runs[0].Tool.Driver.RulesMetadata;

            SelectiveCompare(actualRules, expectedRules);
        }

        private static void SelectiveCompare(IList<Notification> actualNotifications, IList<Notification> expectedNotifications)
        {
            bool actualHasNotifications = actualNotifications != null && actualNotifications.Count > 0;
            bool expectedHasNotifications = expectedNotifications != null && expectedNotifications.Count > 0;
            actualHasNotifications.Should().Be(expectedHasNotifications);

            if (actualHasNotifications && expectedHasNotifications)
            {
                actualNotifications.Count.Should().Be(expectedNotifications.Count);

                for (int i = 0; i < actualNotifications.Count; ++i)
                {
                    Notification actualNotification = actualNotifications[i];
                    Notification expectedNotification = expectedNotifications[i];

                    actualNotification.RuleId.Should().Be(expectedNotification.RuleId);

                    actualNotification.Level.Should().Be(expectedNotification.Level);
                }
            }
        }

        private static void SelectiveCompare(IList<Result> actualResults, IList<Result> expectedResults)
        {
            bool actualHasResults = actualResults != null && actualResults.Count > 0;
            bool expectedHasResults = expectedResults != null && expectedResults.Count > 0;
            actualHasResults.Should().Be(expectedHasResults);

            if (actualHasResults && expectedHasResults)
            {
                actualResults.Count.Should().Be(expectedResults.Count);

                for (int i = 0; i < actualResults.Count; ++i)
                {
                    Result actualResult = actualResults[i];
                    Result expectedResult = expectedResults[i];

                    actualResult.RuleId.Should().Be(expectedResult.RuleId);

                    actualResult.Level.Should().Be(expectedResult.Level);

                    actualResult.Locations[0].PhysicalLocation.Region.ValueEquals(
                        expectedResult.Locations[0].PhysicalLocation.Region).Should().BeTrue();
                }
            }
        }

        private static void SelectiveCompare(IList<MessageDescriptor> actualRules, IList<MessageDescriptor> expectedRules)
        {
            bool actualHasRules = actualRules != null && actualRules.Count > 0;
            bool expectedHasRules = expectedRules != null && expectedRules.Count > 0;
            actualHasRules.Should().Be(expectedHasRules);

            if (actualHasRules && expectedHasRules)
            {
                actualRules.Count.Should().Be(expectedRules.Count);

                for (int i = 0; i < actualRules.Count; ++i)
                {
                    MessageDescriptor actualRule = actualRules[i];
                    MessageDescriptor expectedRule = expectedRules[i];

                    actualRule.Id.Should().Be(expectedRule.Id);
                }
            }
        }
    }
}
