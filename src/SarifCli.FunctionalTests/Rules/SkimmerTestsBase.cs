// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public abstract class SkimmerTestsBase
    {
        private const string JsonSchemaFile = "Sarif.schema.json";
        private const string TestDataDirectory = "TestData";

        protected void Verify(SarifValidationSkimmerBase skimmer, string testFileName)
        {
            string ruleName = skimmer.GetType().Name;
            string testDirectory = Path.Combine(Environment.CurrentDirectory, TestDataDirectory, ruleName);

            string targetPath = Path.Combine(testDirectory, testFileName);
            string expectedFilePath = MakeExpectedFilePath(testDirectory, testFileName);
            string actualFilePath = MakeActualFilePath(testDirectory, testFileName);

            using (var logger = new SarifLogger(
                    actualFilePath,
                    new string[] { targetPath },
                    verbose: false,
                    logEnvironment: false,
                    computeTargetsHash: false,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null))
            {
                logger.AnalysisStarted();

                var context = new SarifValidationContext
                {
                    Rule = skimmer,
                    Logger = logger,
                    TargetUri = new Uri(targetPath),
                    SchemaFilePath = JsonSchemaFile
                };

                skimmer.Initialize(context);
                context.Logger.AnalyzingTarget(context);

                skimmer.Analyze(context);

                logger.AnalysisStopped(RuntimeConditions.None);
            }

            string actualLogContents = File.ReadAllText(actualFilePath);
            string expectedLogContents = File.ReadAllText(expectedFilePath);

            // We can't just compare the text of the log files because properties
            // like start time, and absolute paths, will differ from run to run.
            // Until SarifLogger has a "deterministic" option (see http://github.com/Microsoft/sarif-sdk/issues/500),
            // we perform a selective compare of just the elements we care about.
            SelectiveCompare(actualLogContents, expectedLogContents);
        }

        private static void SelectiveCompare(string actualText, string expectedText)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance
            };

            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(actualText, settings);
            SarifLog expectedLog = JsonConvert.DeserializeObject<SarifLog>(expectedText, settings);

            SelectiveCompare(actualLog, expectedLog);
        }

        private static void SelectiveCompare(SarifLog actualLog, SarifLog expectedLog)
        {
            Result[] actualResults = actualLog.Runs[0].Results.ToArray();
            Result[] expectedResults = expectedLog.Runs[0].Results.ToArray();

            actualResults.Length.Should().Be(expectedResults.Length);

            for (int i = 0; i < actualResults.Length; ++i)
            {
                Result actualResult = actualResults[i];
                Result expectedResult = expectedResults[i];

                actualResult.RuleId.Should().Be(expectedResult.RuleId);

                actualResult.Locations[0].AnalysisTarget.Region.ValueEquals(
                    expectedResult.Locations[0].AnalysisTarget.Region).Should().BeTrue();
            }
        }

        private static string MakeExpectedFilePath(string testDirectory, string testFileName)
        {
            return MakeQualifiedFilePath(testDirectory, testFileName, "Expected");
        }

        private static string MakeActualFilePath(string testDirectory, string testFileName)
        {
            return MakeQualifiedFilePath(testDirectory, testFileName, "Actual");
        }

        private static string MakeQualifiedFilePath(string testDirectory, string testFileName, string qualifier)
        {
            string qualifiedFileName =
                Path.GetFileNameWithoutExtension(testFileName) + "_" + qualifier + Path.GetExtension(testFileName);

            return Path.Combine(testDirectory, qualifiedFileName);
        }
    }
}
