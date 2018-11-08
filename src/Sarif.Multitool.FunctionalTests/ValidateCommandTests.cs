// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Writers;
using System;
using System.IO;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ValidateCommandTests : SarifMultitoolTestBase
    {
        [Fact(DisplayName = nameof(ValidateCommand_ReportsJsonSyntaxError))]
        public void ValidateCommand_ReportsJsonSyntaxError()
        {
            Verify("SyntaxError.sarif", disablePreleaseCompatibilityTransform: true);
        }

        [Fact(DisplayName = nameof(ValidateCommand_ReportsDeserializationError))]
        public void ValidateCommand_ReportsDeserializationError()
        {
            Verify("DeserializationError.sarif");
        }

        private void Verify(string testFileName, bool disablePreleaseCompatibilityTransform = false)
        {
            try
            {
                ValidateCommand.s_DisablePrereleaseCompatibilityTransform = disablePreleaseCompatibilityTransform;
                string testDirectory = Path.Combine(Environment.CurrentDirectory, TestDataDirectory);

                string testFilePath = Path.Combine(TestDataDirectory, testFileName);
                string expectedFilePath = MakeExpectedFilePath(testDirectory, testFileName);
                string actualFilePath = MakeActualFilePath(testDirectory, testFileName);

                var validateOptions = new ValidateOptions
                {
                    SarifVersion = SarifVersion.Current,
                    TargetFileSpecifiers = new[] { testFilePath },
                    OutputFilePath = actualFilePath,
                    SchemaFilePath = JsonSchemaFile,
                    Quiet = true
                };

                new ValidateCommand().Run(validateOptions);

                string actualLogContents = File.ReadAllText(actualFilePath);
                string expectedLogContents = File.ReadAllText(expectedFilePath);
                expectedLogContents = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(expectedLogContents);

                // We can't just compare the text of the log files because properties
                // like start time, and absolute paths, will differ from run to run.
                // Until SarifLogger has a "deterministic" option (see http://github.com/Microsoft/sarif-sdk/issues/500),
                // we perform a selective compare of just the elements we care about.
                SelectiveCompare(actualLogContents, expectedLogContents);
            }
            finally
            {
                ValidateCommand.s_DisablePrereleaseCompatibilityTransform = false;
            }
        }
    }
}
