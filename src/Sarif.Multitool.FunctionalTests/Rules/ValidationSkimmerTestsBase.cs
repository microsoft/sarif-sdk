// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public abstract class ValidationSkimmerTestsBase<TSkimmer> : SarifMultitoolTestBase
        where TSkimmer : SkimmerBase<SarifValidationContext>, new()
    {
        private readonly string _testDirectory;
        private const string ExpectedResultsPropertyName = "expectedResults";

        public ValidationSkimmerTestsBase()
        {
            string ruleName = typeof(TSkimmer).Name;
            _testDirectory = Path.Combine(Environment.CurrentDirectory, TestDataDirectory, ruleName);
        }


        protected void Verify(string testFileName)
        {
            Verify(testFileName, disablePrereleaseCompatibilityTransform: false);
        }

        protected void Verify(string testFileName, bool disablePrereleaseCompatibilityTransform)
        {
            string targetPath = Path.Combine(_testDirectory, testFileName);
            string actualFilePath = MakeActualFilePath(_testDirectory, testFileName);

            string inputLogContents = File.ReadAllText(targetPath);

            if (!disablePrereleaseCompatibilityTransform)
            {
                PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(inputLogContents, forceUpdate: false, formatting: Formatting.Indented, out inputLogContents);
            }

            SarifLog inputLog = JsonConvert.DeserializeObject<SarifLog>(inputLogContents);

            bool expectedResultsArePresent = inputLog.Runs[0].TryGetProperty(ExpectedResultsPropertyName, out ExpectedValidationResults expectedResults);
            expectedResultsArePresent.Should().Be(true);

            var skimmer = new TSkimmer();

            using (var logger = new SarifLogger(
                    actualFilePath,
                    LoggingOptions.None,
                    tool: null,
                    run: null,
                    analysisTargets: new string[] { targetPath },
                    invocationTokensToRedact: null))
            {
                logger.AnalysisStarted();

                var context = new SarifValidationContext
                {
                    Rule = skimmer,
                    Logger = logger,
                    TargetUri = new Uri(targetPath),
                    SchemaFilePath = JsonSchemaFile,
                    InputLogContents = inputLogContents,
                    InputLog = inputLog
                };

                skimmer.Initialize(context);
                context.Logger.AnalyzingTarget(context);

                skimmer.Analyze(context);

                logger.AnalysisStopped(RuntimeConditions.None);
            }

            string actualLogContents = File.ReadAllText(actualFilePath);
            SarifLog outputLog = JsonConvert.DeserializeObject<SarifLog>(actualLogContents);

            Verify(outputLog.Runs[0], expectedResults);
        }

        // Every validation message begins with a placeholder "{0}: " that specifies the
        // result location, for example, "runs[0].results[0].locations[0].physicalLocation".
        // Verify that those detected result locations match the expected locations.
        private void Verify(Run run, ExpectedValidationResults expectedResults)
        {
            HashSet<string> actualResultLocations = new HashSet<string>(run.Results.Select(r => r.Message.Arguments[0]).OrderBy(loc => loc));
            HashSet<string> unexpectedlyAbsentResultLocations = new HashSet<string>(expectedResults.ResultLocationPointers.OrderBy(loc => loc));

            List<string> unexpectedNewIssues = new List<string>();

            foreach(string location in actualResultLocations)
            {
                if (unexpectedlyAbsentResultLocations.Contains(location))
                {
                    // Happy path. Actual location found in expected
                    unexpectedlyAbsentResultLocations.Remove(location);
                }
                else
                {
                    // Bad new unexpected issue 
                    unexpectedNewIssues.Add(location);
                }
            }

            // No new issues
            unexpectedNewIssues.Count.Should().Be(0);

            // No unexpectedly absent issues. If we found everything we expected,
            // then our expected results should be empty
            unexpectedlyAbsentResultLocations.Count.Should().Be(0);
        }
    }
}