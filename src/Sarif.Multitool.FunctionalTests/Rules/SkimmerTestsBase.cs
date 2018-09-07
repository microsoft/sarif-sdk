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
        protected class ExpectedResults
        {
            public int ResultCount { get; set; }
        }

        protected const string ExpectedResultsPropertyName = nameof(ExpectedResults);

        private readonly string _testDirectory;

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolver.Instance
        };

        public SkimmerTestsBase()
        {
            string ruleName = typeof(TSkimmer).Name;
            _testDirectory = Path.Combine(Environment.CurrentDirectory, TestDataDirectory, ruleName);
        }

        protected void Verify(string testFileName)
        {
            var skimmer = new TSkimmer();

            string targetPath = Path.Combine(_testDirectory, testFileName);
            string expectedFilePath = MakeExpectedFilePath(_testDirectory, testFileName);
            string outputFilePath = MakeActualFilePath(_testDirectory, testFileName);

            string inputLogContents = File.ReadAllText(targetPath);

            SarifLog inputLog = JsonConvert.DeserializeObject<SarifLog>(inputLogContents, _settings);

            using (var logger = new SarifLogger(
                    outputFilePath,
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

            string outputLogContents = File.ReadAllText(outputFilePath);
            SarifLog outputLog = JsonConvert.DeserializeObject<SarifLog>(outputLogContents, _settings);

            ExpectedResults expectedResults;
            if (inputLog.Runs[0].TryGetProperty(ExpectedResultsPropertyName, out expectedResults))
            {
                Verify(outputLog.Runs[0], expectedResults);
            }
        }

        private void Verify(Run run, ExpectedResults expectedResults)
        {
            run.Results.Count.Should().Be(expectedResults.ResultCount);
        }
    }
}
