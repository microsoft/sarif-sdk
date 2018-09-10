// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
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

        public SkimmerTestsBase()
        {
            string ruleName = typeof(TSkimmer).Name;
            _testDirectory = Path.Combine(Environment.CurrentDirectory, TestDataDirectory, ruleName);

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };
        }

        protected void Verify(string testFileName)
        {
            string targetPath = Path.Combine(_testDirectory, testFileName);
            string expectedFilePath = MakeExpectedFilePath(_testDirectory, testFileName);
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
            string expectedLogContents = File.ReadAllText(expectedFilePath);

            // We can't just compare the text of the log files because properties
            // like start time, and absolute paths, will differ from run to run.
            // Until SarifLogger has a "deterministic" option (see http://github.com/Microsoft/sarif-sdk/issues/500),
            // we perform a selective compare of just the elements we care about.
            SelectiveCompare(actualLogContents, expectedLogContents);
        }
    }
}
