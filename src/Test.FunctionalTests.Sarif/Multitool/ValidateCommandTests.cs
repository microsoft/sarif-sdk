// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.FunctionalTests.Multitool
{
    public class ValidateCommandTests : FileDiffingFunctionalTests
    {
        public ValidateCommandTests(ITestOutputHelper outputHelper, bool testProducesSarifCurrentVersion = true) :
            base(outputHelper, testProducesSarifCurrentVersion)
        { }

        protected override string IntermediateTestFolder => @"Multitool";

        [Fact]
        public void JSON1001_SyntaxError()
            => RunTest("JSON1001.SyntaxError.sarif");

        [Fact]
        public void JSON1002_DeserializationError()
            => RunTest("JSON1002.DeserializationError.sarif");

        [Fact]
        public void SARIF1001_DoNotUseFriendlyNameAsRuleId_Valid()
            => RunTest(MakeValidTestFileName(RuleId.DoNotUseFriendlyNameAsRuleId, nameof(RuleId.DoNotUseFriendlyNameAsRuleId)));

        [Fact]
        public void SARIF1001_DoNotUseFriendlyNameAsRuleId_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.DoNotUseFriendlyNameAsRuleId, nameof(RuleId.DoNotUseFriendlyNameAsRuleId)));

        [Fact]
        public void SARIF1003_UrisMustBeValid_Valid()
            => RunTest(MakeValidTestFileName(RuleId.UrisMustBeValid, nameof(RuleId.UrisMustBeValid)));

        [Fact]
        public void SARIF1003_UrisMustBeValid_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.UrisMustBeValid, nameof(RuleId.UrisMustBeValid)));

        [Fact]
        public void SARIF1007_EndTimeMustNotBeBeforeStartTime_Valid()
            => RunTest(MakeValidTestFileName(RuleId.EndTimeMustNotBeBeforeStartTime, nameof(RuleId.EndTimeMustNotBeBeforeStartTime)));

        [Fact]
        public void SARIF1007_EndTimeMustNotBeBeforeStartTime_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.EndTimeMustNotBeBeforeStartTime, nameof(RuleId.EndTimeMustNotBeBeforeStartTime)));
        [Fact]
        public void SARIF1008_MessagesShouldEndWithPeriod_Valid()
            => RunTest(MakeValidTestFileName(RuleId.MessagesShouldEndWithPeriod, nameof(RuleId.MessagesShouldEndWithPeriod)));

        [Fact]
        public void SARIF1008_MessagesShouldEndWithPeriod_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.MessagesShouldEndWithPeriod, nameof(RuleId.MessagesShouldEndWithPeriod)));

        /******************
         * This set of tests constructs a full file path that exceeds MAX_PATH when running in some AzureDevOps build and test
         * environments. As a result, we slightly truncate the file names so that they are within ADO's tolerance. If/when
         * we chase down a more satisfying solution, we can restore the nameof() pattern (and updated the corresponding
         * test file names in TestData\Inputs and TestData\ExpectedOutputs.
         ******************/
        [Fact]
        public void SARIF1012_EndLineMustNotBeLessThanStartLine_Valid()
            => RunTest(MakeValidTestFileName(RuleId.EndLineMustNotBeLessThanStartLine, "EndLineMustNotBeLessThanStart"));

        [Fact]
        public void SARIF1012_EndLineMustNotBeLessThanStartLine_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.EndLineMustNotBeLessThanStartLine, "EndLineMustNotBeLessThanStart"));
        [Fact]
        public void SARIF1013_EndColumnMustNotBeLessThanStartColumn_Valid()
            => RunTest(MakeValidTestFileName(RuleId.EndColumnMustNotBeLessThanStartColumn, "EndColumnMustNotBeLessThanStart"));

        [Fact]
        public void SARIF1013_EndColumnMustNotBeLessThanStartColumn_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.EndColumnMustNotBeLessThanStartColumn, "EndColumnMustNotBeLessThanStart"));
        /********** END PROBLEMATIC TESTS*******/

        [Fact]
        public void SARIF1014_UriBaseIdRequiresRelativeUri_Valid()
            => RunTest(MakeValidTestFileName(RuleId.UriBaseIdRequiresRelativeUri, nameof(RuleId.UriBaseIdRequiresRelativeUri)));

        [Fact]
        public void SARIF1014_UriBaseIdRequiresRelativeUri_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.UriBaseIdRequiresRelativeUri, nameof(RuleId.UriBaseIdRequiresRelativeUri)));

        [Fact]
        public void SARIF1015_UriMustBeAbsolute_Valid()
            => RunTest(MakeValidTestFileName(RuleId.UriMustBeAbsolute, nameof(RuleId.UriMustBeAbsolute)));

        [Fact]
        public void SARIF1015_UriMustBeAbsolute_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.UriMustBeAbsolute, nameof(RuleId.UriMustBeAbsolute)));

        [Fact]
        public void SARIF1016_ContextRegionRequiresRegion_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ContextRegionRequiresRegion, nameof(RuleId.ContextRegionRequiresRegion)));

        [Fact]
        public void SARIF1016_ContextRegionRequiresRegion_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ContextRegionRequiresRegion, nameof(RuleId.ContextRegionRequiresRegion)));

        [Fact]
        public void SARIF1017_InvalidIndex_Valid()
            => RunTest(MakeValidTestFileName(RuleId.InvalidIndex, nameof(RuleId.InvalidIndex)));

        [Fact]
        public void SARIF1017_InvalidIndex_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.InvalidIndex, nameof(RuleId.InvalidIndex)));

        [Fact]
        public void SARIF1018_InvalidUriInOriginalUriBaseIds_Valid()
            => RunTest(MakeValidTestFileName(RuleId.InvalidUriInOriginalUriBaseIds, nameof(RuleId.InvalidUriInOriginalUriBaseIds)));

        [Fact]
        public void SARIF1018_InvalidUriInOriginalUriBaseIds_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.InvalidUriInOriginalUriBaseIds, nameof(RuleId.InvalidUriInOriginalUriBaseIds)));

        [Fact]
        public void SARIF1019_RuleIdMustBePresentAndConsistent_Valid()
            => RunTest(MakeValidTestFileName(RuleId.RuleIdMustBePresentAndConsistent, nameof(RuleId.RuleIdMustBePresentAndConsistent)));

        [Fact]
        public void SARIF1019_RuleIdMustBePresentAndConsistent_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.RuleIdMustBePresentAndConsistent, nameof(RuleId.RuleIdMustBePresentAndConsistent)));

        [Fact]
        public void SARIF1020_SchemaMustBePresentAndConsistent_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ReferToFinalSchema, nameof(RuleId.ReferToFinalSchema)));

        [Fact]
        public void SARIF1020_SchemaMustBePresentAndConsistent_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ReferToFinalSchema, nameof(RuleId.ReferToFinalSchema)));

        private const string ValidTestFileNameSuffix = "_Valid.sarif";
        private const string InvalidTestFileNameSuffix = "_Invalid.sarif";

        private string MakeValidTestFileName(string ruleId, string ruleName)
            => $"{ruleId}.{ruleName}{ValidTestFileNameSuffix}";

        private string MakeInvalidTestFileName(string ruleId, string ruleName)
            => $"{ruleId}.{ruleName}{InvalidTestFileNameSuffix}";

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            string v2LogText = GetResourceText(inputResourceName);

            string inputLogDirectory = this.OutputFolderPath;
            string inputLogFileName = Path.GetFileName(inputResourceName);
            string inputLogFilePath = Path.Combine(this.OutputFolderPath, inputLogFileName);

            string actualLogFilePath = Guid.NewGuid().ToString();

            string ruleUnderTest = Path.GetFileNameWithoutExtension(inputLogFilePath).Split('.')[1];

            // All SARIF rule prefixes require update to current release.
            // All rules with JSON prefix are low level syntax/deserialization checks.
            // We can't transform these test inputs as that operation fixes up erros in the file.
            // Also, don't transform the tests for SARIF1020, because that rule examines the actual contents of the $schema
            // property, so we can't change it.
            bool updateInputsToCurrentSarif = ruleUnderTest.StartsWith("SARIF") 
                && ruleUnderTest != "SARIF1020" ? true : false;

            var validateOptions = new ValidateOptions
            {
                SarifOutputVersion = SarifVersion.Current,
                TargetFileSpecifiers = new[] { inputLogFilePath },
                OutputFilePath = actualLogFilePath,
                Quiet = true,
                UpdateInputsToCurrentSarif = updateInputsToCurrentSarif,
                PrettyPrint = true,
                Optimize = true
            };

            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.DirectoryExists(inputLogDirectory)).Returns(true);
            mockFileSystem.Setup(x => x.GetDirectoriesInDirectory(It.IsAny<string>())).Returns(new string[0]);
            mockFileSystem.Setup(x => x.GetFilesInDirectory(inputLogDirectory, inputLogFileName)).Returns(new string[] { inputLogFilePath });
            mockFileSystem.Setup(x => x.ReadAllText(inputLogFilePath)).Returns(v2LogText);
            mockFileSystem.Setup(x => x.ReadAllText(It.IsNotIn<string>(inputLogFilePath))).Returns<string>(path => File.ReadAllText(path));
            mockFileSystem.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));

            var validateCommand = new ValidateCommand(mockFileSystem.Object);

            int returnCode = validateCommand.Run(validateOptions);

            if (validateCommand.ExecutionException != null)
            {
                Console.WriteLine(validateCommand.ExecutionException.ToString());
            }

            returnCode.Should().Be(0);

            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(actualLogFilePath));

            // First, we'll strip any validation results that don't originate with the rule under test
            var newResults = new List<Result>();

            foreach (Result result in actualLog.Runs[0].Results)
            {
                if (result.RuleId == ruleUnderTest)
                {
                    newResults.Add(result);
                }
            }

            // Next, we'll remove non-deterministic information, most notably, timestamps emitted for the invocation data.
            var removeTimestampsVisitor = new RemoveOptionalDataVisitor(OptionallyEmittedData.NondeterministicProperties);
            removeTimestampsVisitor.Visit(actualLog);

            // Finally, we'll elide non-deterministic build root details
            var rebaseUrisVisitor = new RebaseUriVisitor("TEST_DIR", new Uri(inputLogDirectory));
            rebaseUrisVisitor.Visit(actualLog);

            // There are differences in log file output depending on whether we are invoking xunit
            // from within Visual Studio or at the command-line via xunit.exe. We elide these differences.

            ToolComponent driver = actualLog.Runs[0].Tool.Driver;
            driver.Name = "SARIF Functional Testing";
            driver.Version = null;
            driver.FullName = null;
            driver.SemanticVersion = null;
            driver.DottedQuadFileVersion = null;
            driver.Product = null;
            driver.Organization = null;
            driver.Properties?.Clear();
            actualLog.Runs[0].OriginalUriBaseIds = null;

            return JsonConvert.SerializeObject(actualLog, Formatting.Indented);
        }
    }
}
