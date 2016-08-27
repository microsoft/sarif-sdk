// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests
{
    public class AnalyzeCommandTests : SarifCliTestBase
    {
        [Fact(DisplayName = nameof(AnalyzeCommand_ReportsJsonSyntaxError))]
        public void AnalyzeCommand_ReportsJsonSyntaxError()
        {
            const string TestFileName = "SyntaxError.sarif";
            string testDirectory = Path.Combine(Environment.CurrentDirectory, TestDataDirectory);

            string testFilePath = Path.Combine(TestDataDirectory, TestFileName);
            string expectedFilePath = MakeExpectedFilePath(testDirectory, TestFileName);
            string actualFilePath = MakeActualFilePath(testDirectory, TestFileName);

            var analyzeOptions = new AnalyzeOptions
            {
                TargetFileSpecifiers = new[] { testFilePath },
                OutputFilePath = actualFilePath,
                SchemaFilePath = JsonSchemaFile
            };

            new AnalyzeCommand().Run(analyzeOptions);

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
