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
            base(outputHelper, testProducesSarifCurrentVersion) { }

        protected override string IntermediateTestFolder => @"Multitool";

        [Fact]
        public void JSON1001_SyntaxError()
            => RunTest("JSON1001.SyntaxError.sarif");
        [Fact]
        public void JSON1002_DeserializationError()
            => RunTest("JSON1002.DeserializationError.sarif");

        [Fact]
        public void SARIF1001_DoNotUseFriendlyNameAsRuleId_Valid()
            => RunTest(RuleId.DoNotUseFriendlyNameAsRuleId + "." + nameof(RuleId.DoNotUseFriendlyNameAsRuleId) + "_Valid.sarif");

        [Fact]
        public void SARIF1001_DoNotUseFriendlyNameAsRuleId_Invalid()
            => RunTest(RuleId.DoNotUseFriendlyNameAsRuleId + "." + nameof(RuleId.DoNotUseFriendlyNameAsRuleId) + "_Invalid.sarif");

        [Fact]
        public void SARIF1003_UrisMustBeValid_Valid()
            => RunTest(RuleId.UrisMustBeValid + "." + nameof(RuleId.UrisMustBeValid) + "_Valid.sarif");

        [Fact]
        public void SARIF1003_UrisMustBeValid_Invalid()
            => RunTest(RuleId.UrisMustBeValid + "." + nameof(RuleId.UrisMustBeValid) + "_Invalid.sarif");

        [Fact]
        public void SARIF1007_EndTimeMustNotBeBeforeStartTime_Valid()
            => RunTest(RuleId.EndTimeMustNotBeBeforeStartTime + "." + nameof(RuleId.EndTimeMustNotBeBeforeStartTime) + "_Valid.sarif");

        [Fact]
        public void SARIF1007_EndTimeMustNotBeBeforeStartTime_Invalid()
            => RunTest(RuleId.EndTimeMustNotBeBeforeStartTime + "." + nameof(RuleId.EndTimeMustNotBeBeforeStartTime) + "_Invalid.sarif");
        [Fact]
        public void SARIF1008_MessagesShouldEndWithPeriod_Valid()
            => RunTest(RuleId.MessagesShouldEndWithPeriod + "." + nameof(RuleId.MessagesShouldEndWithPeriod) + "_Valid.sarif");

        [Fact]
        public void SARIF1008_MessagesShouldEndWithPeriod_Invalid()
            => RunTest(RuleId.MessagesShouldEndWithPeriod + "." + nameof(RuleId.MessagesShouldEndWithPeriod) + "_Invalid.sarif");
        [Fact]
        public void SARIF1012_EndLineMustNotBeLessThanStartLine_Valid()
            => RunTest(RuleId.EndLineMustNotBeLessThanStartLine + "." + nameof(RuleId.EndLineMustNotBeLessThanStartLine) + "_Valid.sarif");

        [Fact]
        public void SARIF1012_EndLineMustNotBeLessThanStartLine_Invalid()
            => RunTest(RuleId.EndLineMustNotBeLessThanStartLine + "." + nameof(RuleId.EndLineMustNotBeLessThanStartLine) + "_Invalid.sarif");
        [Fact]
        public void SARIF1013_EndColumnMustNotBeLessThanStartColumn_Valid()
            => RunTest(RuleId.EndColumnMustNotBeLessThanStartColumn + "." + nameof(RuleId.EndColumnMustNotBeLessThanStartColumn) + "_Valid.sarif");

        [Fact]
        public void SARIF1013_EndColumnMustNotBeLessThanStartColumn_Invalid()
            => RunTest(RuleId.EndColumnMustNotBeLessThanStartColumn + "." + nameof(RuleId.EndColumnMustNotBeLessThanStartColumn) + "_Invalid.sarif");
        [Fact]
        public void SARIF1014_UriBaseIdRequiresRelativeUri_Valid()
            => RunTest(RuleId.UriBaseIdRequiresRelativeUri + "." + nameof(RuleId.UriBaseIdRequiresRelativeUri) + "_Valid.sarif");

        [Fact]
        public void SARIF1014_UriBaseIdRequiresRelativeUri_Invalid()
            => RunTest(RuleId.UriBaseIdRequiresRelativeUri + "." + nameof(RuleId.UriBaseIdRequiresRelativeUri) + "_Invalid.sarif");

        [Fact]
        public void SARIF1015_UriMustBeAbsolute_Valid()
            => RunTest(RuleId.UriMustBeAbsolute + "." + nameof(RuleId.UriMustBeAbsolute) + "_Valid.sarif");

        [Fact]
        public void SARIF1015_UriMustBeAbsolute_Invalid()
            => RunTest(RuleId.UriMustBeAbsolute + "." + nameof(RuleId.UriMustBeAbsolute) + "_Invalid.sarif");

        [Fact]
        public void SARIF1016_ContextRegionRequiresRegion_Valid()
            => RunTest(RuleId.ContextRegionRequiresRegion + "." + nameof(RuleId.ContextRegionRequiresRegion) + "_Valid.sarif");

        [Fact]
        public void SARIF1016_ContextRegionRequiresRegion_Invalid()
            => RunTest(RuleId.ContextRegionRequiresRegion + "." + nameof(RuleId.ContextRegionRequiresRegion) + "_Invalid.sarif");


        protected override string ConstructTestOutputFromInputResource(string inputResourceName)
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
            bool updateInputsToCurrentSarif = ruleUnderTest.StartsWith("SARIF") ? true : false;

            var validateOptions = new ValidateOptions
            {
                SarifOutputVersion = SarifVersion.Current,
                TargetFileSpecifiers = new[] { inputLogFilePath },
                OutputFilePath = actualLogFilePath,
                Quiet = true,
                UpdateInputsToCurrentSarif = updateInputsToCurrentSarif,
                PrettyPrint = true
            };

            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.DirectoryExists(inputLogDirectory)).Returns(true);
            mockFileSystem.Setup(x => x.GetDirectoriesInDirectory(It.IsAny<string>())).Returns(new string[0]);
            mockFileSystem.Setup(x => x.GetFilesInDirectory(inputLogDirectory, inputLogFileName)).Returns(new string[] { inputLogFilePath });
            mockFileSystem.Setup(x => x.ReadAllText(inputLogFilePath)).Returns(v2LogText);
            mockFileSystem.Setup(x => x.ReadAllText(It.IsNotIn<string>(inputLogFilePath))).Returns<string>(path => File.ReadAllText(path));
            mockFileSystem.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));

            int returnCode = new ValidateCommand(mockFileSystem.Object).Run(validateOptions);
            returnCode.Should().Be(0);

            var actualLog = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(actualLogFilePath));

            // First, we'll strip any validation results that don't originate with the rule under test
            var newResults = new List<Result>();

            foreach (Result result in actualLog.Runs[0].Results)
            {
                if (result.RuleId == ruleUnderTest)
                {
                    newResults.Add(result);
                }
            }

            // There are differences in log file output depending on whether we are invoking xunit
            // from within Visual Studio or at the command-line via xunit.exe. We elide these differences.

            ToolComponent driver = actualLog.Runs[0].Tool.Driver;
            driver.Name = "Sarif Functional Testing";
            driver.Version = null;
            driver.FullName = null;
            driver.SemanticVersion = null;
            driver.DottedQuadFileVersion = null;
            driver.Name = "Sarif Functional Testing";
            driver.Properties.Clear();
            actualLog.Runs[0].OriginalUriBaseIds = null;

            // Next, we'll remove non-deterministic information, most notably, timestamps emitted for the invocation data.
            var removeTimestampsVisitor = new RemoveOptionalDataVisitor(OptionallyEmittedData.NondeterministicProperties);
            removeTimestampsVisitor.Visit(actualLog);

            // Finally, we'll elide non-deterministic build root details
            var rebaseUrisVisitor = new RebaseUriVisitor("TEST_DIR", new Uri(inputLogDirectory));
            rebaseUrisVisitor.Visit(actualLog);

            return JsonConvert.SerializeObject(actualLog, Formatting.Indented);
        }
    }
}
