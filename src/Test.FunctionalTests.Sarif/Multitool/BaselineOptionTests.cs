// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Visitors;

using Moq;

using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Trait(TestTraits.WindowsOnly, "true")]
    public class BaselineOptionTests : FileDiffingFunctionalTests
    {
        private bool IsInline = false;

        public static readonly TestAssetResourceExtractor BaselineNamespaceExtractor =
            new TestAssetResourceExtractor(typeof(BaselineOptionTests).Assembly, testAssetDirectory: "Baseline");

        public BaselineOptionTests(ITestOutputHelper outputHelper, bool testProducesSarifCurrentVersion = true)
            : base(outputHelper, testProducesSarifCurrentVersion)
        {
        }

        [Fact]
        public void TEST1001_ValidateWithBaseline() =>
            RunBaselineOptionTest("TEST1001.ValidateWithBaseline.sarif", "TEST1001");

        [Fact]
        public void TEST1002_ValidateBaseline_NoResults() =>
            // baseline doesn't have results
            // all new result shoule be in new baseline status
            RunBaselineOptionTest("TEST1002.ValidateBaseline.NoResults.sarif", "TEST1002");

        [Fact]
        public void TEST1003_ValidateBaseline_AbsentResults() =>
            // all results of baseline are in absent baseline status.
            // New results should be in New status.
            RunBaselineOptionTest("TEST1003.ValidateBaseline.AbsentResults.sarif", "TEST1003");

        [Fact]
        public void TEST1004_ValidateBaseline_NewResults() =>
            // all results of baseline are in new baseline status.
            // New results should be in unchange status.
            RunBaselineOptionTest("TEST1004.ValidateBaseline.NewResults.sarif", "TEST1004");

        [Fact]
        public void TEST1005_ValidateBaseline_UnchangedResults() =>
            // all results of baseline are in unchanged baseline status.
            // New results should be in unchanged status.
            RunBaselineOptionTest("TEST1005.ValidateBaseline.UnchangedResults.sarif", "TEST1005");

        [Fact]
        public void TEST1006_ValidateBaseline_UpdatedResults() =>
            // New results are different than baseline results, should be in updated status.
            RunBaselineOptionTest("TEST1006.ValidateBaseline.UpdatedResults.sarif", "TEST1006");

        [Fact]
        public void TEST1007_ValidateBaseline_LessResultsThanBaseline() =>
            // New results are less than baseline results (means issue resolved).
            RunBaselineOptionTest("TEST1007.ValidateBaseline.LessResultsThanBaseline.sarif", "TEST1007");


        [Fact]
        public void TEST1008_ValidateBaseline_InlineUpdate() =>
            // Work with inline otpion, will update baseline sarif result with the new results.
            RunBaselineOptionTest("TEST1008.ValidateBaseline.InlineUpdate.sarif", "TEST1008", true);

        private void RunBaselineOptionTest(string testFileName, string testName, bool inline = false)
        {
            this.IsInline = inline;
            RunTest(inputResourceName: testFileName,
                    expectedOutputResourceName: testFileName,
                    parameter: testName);
        }

        protected override string IntermediateTestFolder => @"Multitool";

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            string testName = parameter as string;

            string fileToBeValidated = "ToBeValidated.sarif";
            string filePathToBeValidated = Path.Combine(this.TestOutputDirectory, fileToBeValidated);
            string logText = GetInputSarifTextFromResource(fileToBeValidated);

            string baselineFile = $"{testName}.Baseline.sarif";
            string baselineFilePath = Path.Combine(this.TestOutputDirectory, baselineFile);
            string baselineText = GetInputSarifTextFromResource(baselineFile);

            File.WriteAllText(baselineFilePath, baselineText);

            string inputLogDirectory = this.TestOutputDirectory;
            string inputLogFileName = Path.GetFileName(inputResourceName);
            string inputLogFilePath = Path.Combine(this.TestOutputDirectory, inputLogFileName);

            string outputLogFilePath = Guid.NewGuid().ToString();

            var validateOptions = new ValidateOptions
            {
                SarifOutputVersion = SarifVersion.Current,
                TargetFileSpecifiers = new[] { filePathToBeValidated },
                OutputFilePath = outputLogFilePath,
                BaselineFilePath = baselineFilePath,
                Quiet = true,
                OutputFileOptions = new[] { FilePersistenceOptions.PrettyPrint, FilePersistenceOptions.Optimize, this.IsInline ? FilePersistenceOptions.Inline : FilePersistenceOptions.None },
                Level = new List<FailureLevel> { FailureLevel.Error, FailureLevel.Warning, FailureLevel.Note, FailureLevel.None },
                RuleKindOption = AllRuleKinds// new List<RuleKind>() { RuleKind.Sarif },
            };

            Mock<IFileSystem> mockFileSystem = MockFactory.MakeMockFileSystem();

            mockFileSystem.Setup(x => x.DirectoryExists(inputLogDirectory)).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(Array.Empty<string>());
            mockFileSystem.Setup(x => x.DirectoryGetFiles(inputLogDirectory, fileToBeValidated)).Returns(new string[] { filePathToBeValidated });
            mockFileSystem.Setup(x => x.FileReadAllText(filePathToBeValidated)).Returns(logText);
            mockFileSystem.Setup(x => x.FileOpenRead(filePathToBeValidated)).Returns(new MemoryStream(Encoding.UTF8.GetBytes(filePathToBeValidated)));
            mockFileSystem.Setup(x => x.FileExists(baselineFilePath)).Returns(true);
            mockFileSystem.Setup(x => x.FileExists(filePathToBeValidated)).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(), fileToBeValidated, SearchOption.TopDirectoryOnly)).Returns(new string[] { filePathToBeValidated });
            mockFileSystem.Setup(x => x.FileReadAllText(baselineFilePath)).Returns(baselineText);
            mockFileSystem.Setup(x => x.FileReadAllText(It.IsNotIn<string>(filePathToBeValidated))).Returns<string>(path => File.ReadAllText(path));
            mockFileSystem.Setup(x => x.FileOpenRead(baselineFilePath)).Returns(new MemoryStream(Encoding.UTF8.GetBytes(baselineText)));
            mockFileSystem.Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>()));
            mockFileSystem.Setup(x => x.FileCreate(outputLogFilePath)).Returns((string path) => File.Create(path));
            mockFileSystem.Setup(x => x.FileCreate(baselineFilePath)).Returns((string path) => File.Create(path));
            mockFileSystem.Setup(x => x.FileInfoLength(It.IsAny<string>())).Returns(100);

            var validateCommand = new ValidateCommand();
            var context = new SarifValidationContext { FileSystem = mockFileSystem.Object };
            int returnCode = validateCommand.Run(validateOptions, ref context);
            context.ValidateCommandExecution(returnCode);

            string actualLogFileContents = File.ReadAllText(this.IsInline ? baselineFilePath : outputLogFilePath);
            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(actualLogFileContents);
            Run run = actualLog.Runs[0];

            // Guid/correlation guid/provenance detection times change for every analysis run.
            // Remove these in order not to fail the comparison checks.
            foreach (Result result in run.Results)
            {
                result.Guid = null;
                result.CorrelationGuid = null;
                result.Provenance = null;
                result.SetProperty("ResultMatching", new Dictionary<string, object>());
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
