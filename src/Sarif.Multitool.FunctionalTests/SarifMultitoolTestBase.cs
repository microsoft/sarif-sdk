// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public abstract class SarifMultitoolTestBase
    {
        protected const string JsonSchemaFile = "Sarif.schema.json";
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
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance
            };

            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(actualLogContents, settings);
            SarifLog expectedLog = JsonConvert.DeserializeObject<SarifLog>(expectedLogContents, settings);

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
            Result[] actualResults = SafeListToArray(actualLog.Runs[0].Results);
            Result[] expectedResults = SafeListToArray(expectedLog.Runs[0].Results);

            SelectiveCompare(actualResults, expectedResults);

            Notification[] actualConfigurationNotifications = SafeListToArray(actualLog.Runs[0].Invocation?.ConfigurationNotifications);
            Notification[] expectedConfigurationNotifications = SafeListToArray(expectedLog.Runs[0].Invocation?.ConfigurationNotifications);

            SelectiveCompare(actualConfigurationNotifications, expectedConfigurationNotifications);

            Notification[] actualToolNotifications = SafeListToArray(actualLog.Runs[0].Invocation?.ToolNotifications);
            Notification[] expectedToolNotifications = SafeListToArray(expectedLog.Runs[0].Invocation?.ToolNotifications);

            SelectiveCompare(actualToolNotifications, expectedToolNotifications);

            IDictionary<string, Rule> actualRules = actualLog.Runs[0].Rules;
            IDictionary<string, Rule> expectedRules = expectedLog.Runs[0].Rules;

            SelectiveCompare(actualRules, expectedRules);
        }

        private static void SelectiveCompare(Notification[] actualNotifications, Notification[] expectedNotifications)
        {
            bool actualHasNotifications = actualNotifications != null && actualNotifications.Length > 0;
            bool expectedHasNotifications = expectedNotifications != null && expectedNotifications.Length > 0;
            actualHasNotifications.Should().Be(expectedHasNotifications);

            if (actualHasNotifications && expectedHasNotifications)
            {
                actualNotifications.Length.Should().Be(expectedNotifications.Length);

                for (int i = 0; i < actualNotifications.Length; ++i)
                {
                    Notification actualNotification = actualNotifications[i];
                    Notification expectedNotification = expectedNotifications[i];

                    actualNotification.RuleId.Should().Be(expectedNotification.RuleId);

                    actualNotification.Level.Should().Be(expectedNotification.Level);
                }
            }
        }

        private static void SelectiveCompare(Result[] actualResults, Result[] expectedResults)
        {
            bool actualHasResults = actualResults != null && actualResults.Length > 0;
            bool expectedHasResults = expectedResults != null && expectedResults.Length > 0;
            actualHasResults.Should().Be(expectedHasResults);

            if (actualHasResults && expectedHasResults)
            {
                actualResults.Length.Should().Be(expectedResults.Length);

                for (int i = 0; i < actualResults.Length; ++i)
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

        private static void SelectiveCompare(IDictionary<string, Rule> actualRules, IDictionary<string, Rule> expectedRules)
        {
            bool actualHasRules = actualRules != null && actualRules.Count > 0;
            bool expectedHasRules = expectedRules != null && expectedRules.Count > 0;
            actualHasRules.Should().Be(expectedHasRules);

            if (actualHasRules && expectedHasRules)
            {
                actualRules.Count.Should().Be(expectedRules.Count);

                foreach (string key in actualRules.Keys)
                {
                    Rule actualRule = actualRules[key];
                    Rule expectedRule;
                    expectedRules.TryGetValue(key, out expectedRule).Should().BeTrue();

                    actualRule.Id.Should().Be(expectedRule.Id);
                }
            }
        }

        private static T[] SafeListToArray<T>(IList<T> list)
        {
            return list == null ? null : list.ToArray();
        }
    }
}
