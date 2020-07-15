// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private class TestParameters
        {
            internal bool Verbose { get; }
            internal string ConfigFileName { get; }

            internal TestParameters(bool verbose = false, string configFileName = null)
            {
                Verbose = verbose;
                ConfigFileName = configFileName ?? Path.Combine(Directory.GetCurrentDirectory(), "default.configuration.xml");
            }
        }

        private static readonly TestParameters s_defaultTestParameters = new TestParameters();

        [Fact]
        public void JSON1001_SyntaxError()
            => RunTest("JSON1001.SyntaxError.sarif");

        [Fact]
        public void JSON1002_DeserializationError()
            => RunTest("JSON1002.DeserializationError.sarif");

        [Fact]
        public void SARIF1001_RuleIdentifiersMustBeValid_Valid()
            => RunTest(MakeValidTestFileName(RuleId.RuleIdentifiersMustBeValid, nameof(RuleId.RuleIdentifiersMustBeValid)));

        [Fact]
        public void SARIF1001_RuleIdentifiersMustBeValid_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.RuleIdentifiersMustBeValid, nameof(RuleId.RuleIdentifiersMustBeValid)));

        [Fact]
        public void SARIF1002_UrisMustBeValid_Valid()
            => RunTest(MakeValidTestFileName(RuleId.UrisMustBeValid, nameof(RuleId.UrisMustBeValid)));

        [Fact]
        public void SARIF1002_UrisMustBeValid_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.UrisMustBeValid, nameof(RuleId.UrisMustBeValid)));

        [Fact]
        public void SARIF1004_ExpressUriBaseIdsCorrectly_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ExpressUriBaseIdsCorrectly, nameof(RuleId.ExpressUriBaseIdsCorrectly)));

        [Fact]
        public void SARIF1004_ExpressUriBaseIdsCorrectly_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ExpressUriBaseIdsCorrectly, nameof(RuleId.ExpressUriBaseIdsCorrectly)));

        [Fact]
        public void SARIF1005_UriMustBeAbsolute_Valid()
            => RunTest(MakeValidTestFileName(RuleId.UriMustBeAbsolute, nameof(RuleId.UriMustBeAbsolute)));

        [Fact]
        public void SARIF1005_UriMustBeAbsolute_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.UriMustBeAbsolute, nameof(RuleId.UriMustBeAbsolute)));

        [Fact]
        public void SARIF1006_InvocationPropertiesMustBeConsistent_Valid()
            => RunTest(MakeValidTestFileName(RuleId.InvocationPropertiesMustBeConsistent, nameof(RuleId.InvocationPropertiesMustBeConsistent)));

        [Fact]
        public void SARIF1006_InvocationPropertiesMustBeConsistent_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.InvocationPropertiesMustBeConsistent, nameof(RuleId.InvocationPropertiesMustBeConsistent)));

        /******************
         * This set of tests constructs a full file path that exceeds MAX_PATH when running in some AzureDevOps build and test
         * environments. As a result, we slightly truncate the file names so that they are within ADO's tolerance. If/when
         * we chase down a more satisfying solution, we can restore the nameof() pattern (and updated the corresponding
         * test file names in TestData\Inputs and TestData\ExpectedOutputs.
         ******************/
        [Fact]
        public void SARIF1007_RegionPropertiesMustBeConsistent_Valid()
            => RunTest(MakeValidTestFileName(RuleId.RegionPropertiesMustBeConsistent, "RegionPropertiesMustBeConsistent"));

        [Fact]
        public void SARIF1007_RegionPropertiesMustBeConsistent_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.RegionPropertiesMustBeConsistent, "RegionPropertiesMustBeConsistent"));

        /********** END PROBLEMATIC TESTS*******/

        [Fact]
        public void SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Valid()
            => RunTest(MakeValidTestFileName(RuleId.PhysicalLocationPropertiesMustBeConsistent, nameof(RuleId.PhysicalLocationPropertiesMustBeConsistent)));

        [Fact]
        public void SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.PhysicalLocationPropertiesMustBeConsistent, nameof(RuleId.PhysicalLocationPropertiesMustBeConsistent)));

        [Fact]
        public void SARIF1009_IndexPropertiesMustBeConsistentWithArrays_Valid()
            => RunTest(MakeValidTestFileName(RuleId.IndexPropertiesMustBeConsistentWithArrays, nameof(RuleId.IndexPropertiesMustBeConsistentWithArrays)));

        [Fact]
        public void SARIF1009_IndexPropertiesMustBeConsistentWithArrays_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.IndexPropertiesMustBeConsistentWithArrays, nameof(RuleId.IndexPropertiesMustBeConsistentWithArrays)));

        [Fact]
        public void SARIF1010_RuleIdMustBeConsistent_Valid()
            => RunTest(MakeValidTestFileName(RuleId.RuleIdMustBeConsistent, nameof(RuleId.RuleIdMustBeConsistent)));

        [Fact]
        public void SARIF1010_RuleIdMustBeConsistent_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.RuleIdMustBeConsistent, nameof(RuleId.RuleIdMustBeConsistent)));

        [Fact]
        public void SARIF1011_ReferenceFinalSchema_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ReferenceFinalSchema, nameof(RuleId.ReferenceFinalSchema)));

        [Fact]
        public void SARIF1011_ReferenceFinalSchema_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ReferenceFinalSchema, nameof(RuleId.ReferenceFinalSchema)));

        [Fact]
        public void SARIF1012_MessageArgumentsMustBeConsistentWithRule_Valid()
            => RunTest(MakeValidTestFileName(RuleId.MessageArgumentsMustBeConsistentWithRule, nameof(RuleId.MessageArgumentsMustBeConsistentWithRule)));

        [Fact]
        public void SARIF1012_MessageArgumentsMustBeConsistentWithRule_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.MessageArgumentsMustBeConsistentWithRule, nameof(RuleId.MessageArgumentsMustBeConsistentWithRule)));

        [Fact]
        public void SARIF2001_TerminateMessagesWithPeriod_Valid()
            => RunTest(MakeValidTestFileName(RuleId.TerminateMessagesWithPeriod, nameof(RuleId.TerminateMessagesWithPeriod)));

        [Fact]
        public void SARIF2001_TerminateMessagesWithPeriod_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.TerminateMessagesWithPeriod, nameof(RuleId.TerminateMessagesWithPeriod)));

        [Fact]
        public void SARIF2002_ProvideMessageArguments_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ProvideMessageArguments, nameof(RuleId.ProvideMessageArguments)),
                parameter: new TestParameters(configFileName: "enable2002.configuration.xml"));

        [Fact]
        public void SARIF2002_ProvideMessageArguments_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ProvideMessageArguments, nameof(RuleId.ProvideMessageArguments)),
                parameter: new TestParameters(configFileName: "enable2002.configuration.xml"));

        [Fact]
        public void SARIF2003_ProvideVersionControlProvenance_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ProvideVersionControlProvenance, nameof(RuleId.ProvideVersionControlProvenance)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2003_ProvideVersionControlProvenance_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ProvideVersionControlProvenance, nameof(RuleId.ProvideVersionControlProvenance)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2004_OptimizeFileSize_Valid()
            => RunTest(MakeValidTestFileName(RuleId.OptimizeFileSize, nameof(RuleId.OptimizeFileSize)));

        [Fact]
        public void SARIF2004_OptimizeFileSize_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.OptimizeFileSize, nameof(RuleId.OptimizeFileSize)));

        [Fact]
        public void SARIF2005_ProvideToolProperties_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ProvideToolProperties, nameof(RuleId.ProvideToolProperties)));

        [Fact]
        public void SARIF2005_ProvideToolProperties_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ProvideToolProperties, nameof(RuleId.ProvideToolProperties)));

        [Fact]
        public void SARIF2006_UrisShouldBeReachable_Valid()
            => RunTest(MakeValidTestFileName(RuleId.UrisShouldBeReachable, nameof(RuleId.UrisShouldBeReachable)),
                parameter: new TestParameters(configFileName: "enable2006.configuration.xml"));

        [Fact]
        public void SARIF2006_UrisShouldBeReachable_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.UrisShouldBeReachable, nameof(RuleId.UrisShouldBeReachable)),
                parameter: new TestParameters(configFileName: "enable2006.configuration.xml"));

        [Fact]
        public void SARIF2007_ExpressPathsRelativeToRepoRoot_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ExpressPathsRelativeToRepoRoot, nameof(RuleId.ExpressPathsRelativeToRepoRoot)),
                parameter: new TestParameters(configFileName: "enable2007.configuration.xml"));

        [Fact]
        public void SARIF2007_ExpressPathsRelativeToRepoRoot_WithoutVersionControlProvenance_Valid()
            => RunTest("SARIF2007.ExpressPathsRelativeToRepoRoot_WithoutVersionControlProvenance_Valid.sarif",
                parameter: new TestParameters(configFileName: "enable2007.configuration.xml"));

        [Fact]
        public void SARIF2007_ExpressPathsRelativeToRepoRoot_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ExpressPathsRelativeToRepoRoot, nameof(RuleId.ExpressPathsRelativeToRepoRoot)),
                parameter: new TestParameters(configFileName: "enable2007.configuration.xml"));

        [Fact]
        public void SARIF2008_ProvideSchema_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ProvideSchema, nameof(RuleId.ProvideSchema)));

        [Fact]
        public void SARIF2008_ProvideSchema_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ProvideSchema, nameof(RuleId.ProvideSchema)));

        [Fact]
        public void SARIF2009_ConsiderConventionalIdentifierValues_Valid()
            => RunTest(
                MakeValidTestFileName(RuleId.ConsiderConventionalIdentifierValues, nameof(RuleId.ConsiderConventionalIdentifierValues)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2009_ConsiderConventionalIdentifierValues_Invalid()
            => RunTest(
                MakeInvalidTestFileName(RuleId.ConsiderConventionalIdentifierValues, nameof(RuleId.ConsiderConventionalIdentifierValues)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2010_ProvideCodeSnippets_Valid()
            => RunTest(
                MakeValidTestFileName(RuleId.ProvideCodeSnippets, nameof(RuleId.ProvideCodeSnippets)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2010_ProvideCodeSnippets_Invalid()
            => RunTest(
                MakeInvalidTestFileName(RuleId.ProvideCodeSnippets, nameof(RuleId.ProvideCodeSnippets)),
                parameter: new TestParameters(verbose: true, configFileName: "disable2011.configuration.xml"));

        [Fact]
        public void SARIF2011_ProvideContextRegion_Valid()
            => RunTest(
                MakeValidTestFileName(RuleId.ProvideContextRegion, nameof(RuleId.ProvideContextRegion)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2011_ProvideContextRegion_Invalid()
            => RunTest(
                MakeInvalidTestFileName(RuleId.ProvideContextRegion, nameof(RuleId.ProvideContextRegion)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2012_ProvideHelpUris_Valid()
            => RunTest(
                MakeValidTestFileName(RuleId.ProvideHelpUris, nameof(RuleId.ProvideHelpUris)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2012_ProvideHelpUris_Invalid()
            => RunTest(
                MakeInvalidTestFileName(RuleId.ProvideHelpUris, nameof(RuleId.ProvideHelpUris)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2013_ProvideEmbeddedFileContent_Valid()
            => RunTest(
                MakeValidTestFileName(RuleId.ProvideEmbeddedFileContent, nameof(RuleId.ProvideEmbeddedFileContent)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2013_ProvideEmbeddedFileContent_Invalid()
            => RunTest(
                MakeInvalidTestFileName(RuleId.ProvideEmbeddedFileContent, nameof(RuleId.ProvideEmbeddedFileContent)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2014_ProvideDynamicMessageContent_Valid()
            => RunTest(MakeValidTestFileName(RuleId.ProvideDynamicMessageContent, nameof(RuleId.ProvideDynamicMessageContent)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2014_ProvideDynamicMessageContent_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.ProvideDynamicMessageContent, nameof(RuleId.ProvideDynamicMessageContent)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2015_EnquoteDynamicMessageContent_Valid()
            => RunTest(MakeValidTestFileName(RuleId.EnquoteDynamicMessageContent, nameof(RuleId.EnquoteDynamicMessageContent)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2015_EnquoteDynamicMessageContent_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.EnquoteDynamicMessageContent, nameof(RuleId.EnquoteDynamicMessageContent)),
                parameter: new TestParameters(verbose: true));

        [Fact]
        public void SARIF2016_FileUrisShouldBeRelative_Valid()
            => RunTest(MakeValidTestFileName(RuleId.FileUrisShouldBeRelative, nameof(RuleId.FileUrisShouldBeRelative)),
                parameter: new TestParameters(verbose: true, configFileName: "enable2016.configuration.xml"));

        [Fact]
        public void SARIF2016_FileUrisShouldBeRelative_Invalid()
            => RunTest(MakeInvalidTestFileName(RuleId.FileUrisShouldBeRelative, nameof(RuleId.FileUrisShouldBeRelative)),
                parameter: new TestParameters(verbose: true, configFileName: "enable2016.configuration.xml"));

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
            // We can't transform these test inputs as that operation fixes up errors in the file.
            // Also, don't transform the tests for SARIF1011 or SARIF2008, because these rules
            // examine the actual contents of the $schema property.

            string[] shouldNotTransform = { "SARIF1011", "SARIF2008" };

            bool updateInputsToCurrentSarif = ruleUnderTest.StartsWith("SARIF") 
                && !shouldNotTransform.Contains(ruleUnderTest);

            TestParameters testParameters = parameter != null
                ? (TestParameters)parameter
                : s_defaultTestParameters;

            var validateOptions = new ValidateOptions
            {
                SarifOutputVersion = SarifVersion.Current,
                TargetFileSpecifiers = new[] { inputLogFilePath },
                OutputFilePath = actualLogFilePath,
                Quiet = true,
                UpdateInputsToCurrentSarif = updateInputsToCurrentSarif,
                PrettyPrint = true,
                Optimize = true,
                Verbose = testParameters.Verbose,
                ConfigurationFilePath = testParameters.ConfigFileName
            };

            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.DirectoryExists(inputLogDirectory)).Returns(true);
            mockFileSystem.Setup(x => x.GetDirectoriesInDirectory(It.IsAny<string>())).Returns(new string[0]);
            mockFileSystem.Setup(x => x.GetFilesInDirectory(inputLogDirectory, inputLogFileName)).Returns(new string[] { inputLogFilePath });
            mockFileSystem.Setup(x => x.ReadAllText(inputLogFilePath)).Returns(v2LogText);
            mockFileSystem.Setup(x => x.ReadAllText(It.IsNotIn<string>(inputLogFilePath))).Returns<string>(path => File.ReadAllText(path));
            mockFileSystem.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));
            mockFileSystem.Setup(x => x.FileExists(validateOptions.ConfigurationFilePath)).Returns(true);

            var validateCommand = new ValidateCommand(mockFileSystem.Object);

            int returnCode = validateCommand.Run(validateOptions);

            if (validateCommand.ExecutionException != null)
            {
                Console.WriteLine(validateCommand.ExecutionException.ToString());
            }

            returnCode.Should().Be(0);

            string actualLogFileContents = File.ReadAllText(actualLogFilePath);
            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(actualLogFileContents);

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
