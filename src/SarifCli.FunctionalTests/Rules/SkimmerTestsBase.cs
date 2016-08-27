// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public abstract class SkimmerTestsBase : SarifCliTestBase
    {
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
    }
}
