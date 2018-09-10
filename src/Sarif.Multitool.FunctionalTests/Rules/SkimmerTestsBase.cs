// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public abstract class SkimmerTestsBase<TSkimmer> : SarifMultitoolTestBase
        where TSkimmer : SkimmerBase<SarifValidationContext>, new()
    {
        private readonly string _testDirectory;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private const string ExpectedResultsPropertyName = "expectedResults";

        public SkimmerTestsBase()
        {
            string ruleName = typeof(TSkimmer).Name;
            _testDirectory = Path.Combine(Environment.CurrentDirectory, TestDataDirectory, ruleName);

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };
        }

        // For the moment, we support two different test designs.
        //
        // The new, preferred design (all new tests should be written this way):
        //
        // The test file itself contains a custom property that summarizes the expected
        // results of running the rule on the test file.
        //
        // The old, deprecated design:
        //
        // To each test file there exists a corresponding file whose name ends in
        // "_Expected.sarif" that contains the expected results of running the rule
        // on the test file. We perform a "selective compare" of the expected and
        // actual validation log file contents.
        //
        // As we migrate from the old to the new design, if the custom property exists,
        // we use the new design, if the "expected" file exists, we use the old design,
        // and if both the custom property and the "expected" file exist, we execute
        // both the new and the old style tests.
        protected void Verify(string testFileName)
        {
            string targetPath = Path.Combine(_testDirectory, testFileName);
            string actualFilePath = MakeActualFilePath(_testDirectory, testFileName);

            string inputLogContents = File.ReadAllText(targetPath);

            SarifLog inputLog = JsonConvert.DeserializeObject<SarifLog>(inputLogContents, _jsonSerializerSettings);

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

            string expectedFilePath = MakeExpectedFilePath(_testDirectory, testFileName);
            if (File.Exists(expectedFilePath))
            {
                // The "expected" file exists. Use the old, deprecated verification method.
                string expectedLogContents = File.ReadAllText(expectedFilePath);

                // We can't just compare the text of the log files because properties
                // like start time, and absolute paths, will differ from run to run.
                // Until SarifLogger has a "deterministic" option (see http://github.com/Microsoft/sarif-sdk/issues/500),
                // we perform a selective compare of just the elements we care about.
                SelectiveCompare(actualLogContents, expectedLogContents);
            }

            if (inputLog.Runs[0].TryGetProperty(ExpectedResultsPropertyName, out ExpectedValidationResults expectedResults))
            {
                // The custom property exists. Use the new, preferred verification method.
                SarifLog outputLog = JsonConvert.DeserializeObject<SarifLog>(actualLogContents, _jsonSerializerSettings);
                Verify(outputLog.Runs[0], expectedResults);
            }
        }

        private void Verify(Run run, ExpectedValidationResults expectedResults)
        {
            run.Results.Count.Should().Be(expectedResults.ResultCount);
        }
    }
}
