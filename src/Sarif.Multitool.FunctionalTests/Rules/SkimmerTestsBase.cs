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
    public abstract class SkimmerTestsBase : SarifMultitoolTestBase
    {
        protected void Verify(SkimmerBase<SarifValidationContext> skimmer, string testFileName)
        {
            string ruleName = skimmer.GetType().Name;
            string testDirectory = Path.Combine(Environment.CurrentDirectory, TestDataDirectory, ruleName);

            string targetPath = Path.Combine(testDirectory, testFileName);
            string expectedFilePath = MakeExpectedFilePath(testDirectory, testFileName);
            string actualFilePath = MakeActualFilePath(testDirectory, testFileName);

            string inputLogContents = File.ReadAllText(targetPath);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };

            SarifLog inputLog = JsonConvert.DeserializeObject<SarifLog>(inputLogContents, settings);

            using (var logger = new SarifLogger(
                    actualFilePath,
                    LoggingOptions.None,
                    tool: null,
                    run: null,                
                    analysisTargets: new string[] { targetPath },
                    prereleaseInfo: null,
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
