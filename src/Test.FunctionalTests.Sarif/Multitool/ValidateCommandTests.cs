// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
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
        private readonly IList<SarifValidationSkimmerBase> validationRules;

        public ValidateCommandTests(ITestOutputHelper outputHelper, bool testProducesSarifCurrentVersion = true) :
            base(outputHelper, testProducesSarifCurrentVersion)
        {
            this.validationRules = GetValidationRules();
        }

        protected override string IntermediateTestFolder => @"Multitool";

        [Fact]
        public void JSON1001_SyntaxError()
            => RunTest("JSON1001.SyntaxError.sarif");

        [Fact]
        public void JSON1002_DeserializationError()
            => RunTest("JSON1002.DeserializationError.sarif");

        [Fact]
        public void SARIF1001_RuleIdentifiersMustBeValid_Valid()
            => RunValidTestForRule(RuleId.RuleIdentifiersMustBeValid);

        [Fact]
        public void SARIF1001_RuleIdentifiersMustBeValid_Invalid()
            => RunInvalidTestForRule(RuleId.RuleIdentifiersMustBeValid);

        [Fact]
        public void SARIF1002_UrisMustBeValid_Valid()
            => RunValidTestForRule(RuleId.UrisMustBeValid);

        [Fact]
        public void SARIF1002_UrisMustBeValid_Invalid()
            => RunInvalidTestForRule(RuleId.UrisMustBeValid);

        [Fact]
        public void SARIF1004_ExpressUriBaseIdsCorrectly_Valid()
            => RunValidTestForRule(RuleId.ExpressUriBaseIdsCorrectly);

        [Fact]
        public void SARIF1004_ExpressUriBaseIdsCorrectly_Invalid()
            => RunInvalidTestForRule(RuleId.ExpressUriBaseIdsCorrectly);

        [Fact]
        public void SARIF1005_UriMustBeAbsolute_Valid()
            => RunValidTestForRule(RuleId.UriMustBeAbsolute);

        [Fact]
        public void SARIF1005_UriMustBeAbsolute_Invalid()
            => RunInvalidTestForRule(RuleId.UriMustBeAbsolute);

        [Fact]
        public void SARIF1006_InvocationPropertiesMustBeConsistent_Valid()
            => RunValidTestForRule(RuleId.InvocationPropertiesMustBeConsistent);

        [Fact]
        public void SARIF1006_InvocationPropertiesMustBeConsistent_Invalid()
            => RunInvalidTestForRule(RuleId.InvocationPropertiesMustBeConsistent);

        [Fact]
        public void SARIF1007_RegionPropertiesMustBeConsistent_Valid()
            => RunValidTestForRule(RuleId.RegionPropertiesMustBeConsistent);

        [Fact]
        public void SARIF1007_RegionPropertiesMustBeConsistent_Invalid()
            => RunInvalidTestForRule(RuleId.RegionPropertiesMustBeConsistent);

        [Fact]
        public void SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Valid()
            => RunValidTestForRule(RuleId.PhysicalLocationPropertiesMustBeConsistent);

        [Fact]
        public void SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Invalid()
            => RunInvalidTestForRule(RuleId.PhysicalLocationPropertiesMustBeConsistent);

        [Fact]
        public void SARIF1009_IndexPropertiesMustBeConsistentWithArrays_Valid()
            => RunValidTestForRule(RuleId.IndexPropertiesMustBeConsistentWithArrays);

        [Fact]
        public void SARIF1009_IndexPropertiesMustBeConsistentWithArrays_Invalid()
            => RunInvalidTestForRule(RuleId.IndexPropertiesMustBeConsistentWithArrays);

        [Fact]
        public void SARIF1010_RuleIdMustBeConsistent_Valid()
            => RunValidTestForRule(RuleId.RuleIdMustBeConsistent);

        [Fact]
        public void SARIF1010_RuleIdMustBeConsistent_Invalid()
            => RunInvalidTestForRule(RuleId.RuleIdMustBeConsistent);

        [Fact]
        public void SARIF1011_ReferenceFinalSchema_Valid()
            => RunValidTestForRule(RuleId.ReferenceFinalSchema);

        [Fact]
        public void SARIF1011_ReferenceFinalSchema_Invalid()
            => RunInvalidTestForRule(RuleId.ReferenceFinalSchema);

        [Fact]
        public void SARIF1012_MessageArgumentsMustBeConsistentWithRule_Valid()
            => RunValidTestForRule(RuleId.MessageArgumentsMustBeConsistentWithRule);

        [Fact]
        public void SARIF1012_MessageArgumentsMustBeConsistentWithRule_Invalid()
            => RunInvalidTestForRule(RuleId.MessageArgumentsMustBeConsistentWithRule);

        [Fact]
        public void SARIF2001_TerminateMessagesWithPeriod_Valid()
            => RunValidTestForRule(RuleId.TerminateMessagesWithPeriod);

        [Fact]
        public void SARIF2001_TerminateMessagesWithPeriod_Invalid()
            => RunInvalidTestForRule(RuleId.TerminateMessagesWithPeriod);

        [Fact]
        public void SARIF2002_ProvideMessageArguments_Valid()
            => RunValidTestForRule(RuleId.ProvideMessageArguments);

        [Fact]
        public void SARIF2002_ProvideMessageArguments_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideMessageArguments);

        [Fact]
        public void SARIF2003_ProvideVersionControlProvenance_Valid()
            => RunValidTestForRule(RuleId.ProvideVersionControlProvenance);

        [Fact]
        public void SARIF2003_ProvideVersionControlProvenance_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideVersionControlProvenance);

        [Fact]
        public void SARIF2004_OptimizeFileSize_Valid()
            => RunValidTestForRule(RuleId.OptimizeFileSize);

        [Fact]
        public void SARIF2004_OptimizeFileSize_Invalid()
            => RunInvalidTestForRule(RuleId.OptimizeFileSize);

        [Fact]
        public void SARIF2005_ProvideToolProperties_Valid()
            => RunValidTestForRule(RuleId.ProvideToolProperties);

        [Fact]
        public void SARIF2005_ProvideToolProperties_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideToolProperties);

        [Fact]
        public void SARIF2006_UrisShouldBeReachable_Valid()
            => RunValidTestForRule(RuleId.UrisShouldBeReachable);

        [Fact]
        public void SARIF2006_UrisShouldBeReachable_Invalid()
            => RunInvalidTestForRule(RuleId.UrisShouldBeReachable);

        [Fact]
        public void SARIF2007_ExpressPathsRelativeToRepoRoot_Valid()
            => RunValidTestForRule(RuleId.ExpressPathsRelativeToRepoRoot);

        [Fact]
        public void SARIF2007_ExpressPathsRelativeToRepoRoot_WithoutVersionControlProvenance_Valid()
            => RunTest("SARIF2007.ExpressPathsRelativeToRepoRoot_WithoutVersionControlProvenance_Valid.sarif");

        [Fact]
        public void SARIF2007_ExpressPathsRelativeToRepoRoot_Invalid()
            => RunInvalidTestForRule(RuleId.ExpressPathsRelativeToRepoRoot);

        [Fact]
        public void SARIF2008_ProvideSchema_Valid()
            => RunValidTestForRule(RuleId.ProvideSchema);

        [Fact]
        public void SARIF2008_ProvideSchema_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideSchema);

        [Fact]
        public void SARIF2009_ConsiderConventionalIdentifierValues_Valid()
            => RunValidTestForRule(RuleId.ConsiderConventionalIdentifierValues);

        [Fact]
        public void SARIF2009_ConsiderConventionalIdentifierValues_Invalid()
            => RunInvalidTestForRule(RuleId.ConsiderConventionalIdentifierValues);

        [Fact]
        public void SARIF2010_ProvideCodeSnippets_Valid()
            => RunValidTestForRule(RuleId.ProvideCodeSnippets);

        [Fact]
        public void SARIF2010_ProvideCodeSnippets_WithEmbeddedContent_Valid()
            => RunTest("SARIF2010.ProvideCodeSnippets_WithEmbeddedContent.sarif");

        [Fact]
        public void SARIF2010_ProvideCodeSnippets_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideCodeSnippets);

        [Fact]
        public void SARIF2011_ProvideContextRegion_Valid()
            => RunValidTestForRule(RuleId.ProvideContextRegion);

        [Fact]
        public void SARIF2011_ProvideContextRegion_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideContextRegion);

        [Fact]
        public void SARIF2012_ProvideHelpUris_Valid()
            => RunValidTestForRule(RuleId.ProvideHelpUris);

        [Fact]
        public void SARIF2012_ProvideHelpUris_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideHelpUris);

        [Fact]
        public void SARIF2013_ProvideEmbeddedFileContent_Valid()
            => RunValidTestForRule(RuleId.ProvideEmbeddedFileContent);

        [Fact]
        public void SARIF2013_ProvideEmbeddedFileContent_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideEmbeddedFileContent);

        [Fact]
        public void SARIF2014_ProvideDynamicMessageContent_Valid()
            => RunValidTestForRule(RuleId.ProvideDynamicMessageContent);

        [Fact]
        public void SARIF2014_ProvideDynamicMessageContent_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideDynamicMessageContent);

        [Fact]
        public void SARIF2015_EnquoteDynamicMessageContent_Valid()
            => RunValidTestForRule(RuleId.EnquoteDynamicMessageContent);

        [Fact]
        public void SARIF2015_EnquoteDynamicMessageContent_Invalid()
            => RunInvalidTestForRule(RuleId.EnquoteDynamicMessageContent);

        [Fact]
        public void SARIF2016_FileUrisShouldBeRelative_Valid()
            => RunValidTestForRule(RuleId.FileUrisShouldBeRelative);

        [Fact]
        public void SARIF2016_FileUrisShouldBeRelative_Invalid()
            => RunInvalidTestForRule(RuleId.FileUrisShouldBeRelative);

        private const string ValidTestFileNameSuffix = "_Valid.sarif";
        private const string InvalidTestFileNameSuffix = "_Invalid.sarif";

        private void RunValidTestForRule(string ruleId)
            => RunTestForRule(ruleId, ValidTestFileNameSuffix);
        
        private void RunInvalidTestForRule(string ruleId)
            => RunTestForRule(ruleId, InvalidTestFileNameSuffix);

        private void RunTestForRule(string ruleId, string testFileNameSuffix)
        {
            SarifValidationSkimmerBase rule = this.validationRules.Single(vr => vr.Id == ruleId);
            string testFileName = MakeTestFileName(rule, testFileNameSuffix);
            RunTest(testFileName);
        }

        private string MakeTestFileName(ReportingDescriptor rule, string testFileNameSuffix)
            => $"{rule.Id}.{rule.Name}{testFileNameSuffix}";

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

            var validateOptions = new ValidateOptions
            {
                SarifOutputVersion = SarifVersion.Current,
                TargetFileSpecifiers = new[] { inputLogFilePath },
                OutputFilePath = actualLogFilePath,
                Quiet = true,
                UpdateInputsToCurrentSarif = updateInputsToCurrentSarif,
                PrettyPrint = true,
                Optimize = true,
                Verbose = true // Turn on note-level rules.
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
            Run run = actualLog.Runs[0];

            // First, we'll strip any validation results that don't originate with the rule under test.
            // But leave the results that originate from JSchema! Also, be careful because output files
            // from "valid" test cases don't have any results.
            run.Results = run.Results
                ?.Where(r => IsRelevant(r.RuleId, ruleUnderTest))
                ?.ToList();

            // Next, remove any rule metadata for those rules. The output files from "valid" test
            // cases don't have any rules.
            run.Tool.Driver.Rules = run.Tool.Driver.Rules
                ?.Where(r => IsRelevant(r.Id, ruleUnderTest))
                ?.ToList();

            // Since there's only one rule in the metadata, the ruleIndex for all remaining results
            // must be 0.
            foreach (Result result in run.Results)
            {
                result.RuleIndex = 0;
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

        private IList<SarifValidationSkimmerBase> GetValidationRules()
        {
            // Select one rule arbitrarily, find out what assembly it's in, and get all the other
            // rules from that assembly.
            Assembly validationRuleAssembly = typeof(RuleIdentifiersMustBeValid).Assembly;
            
            return CompositionUtilities.GetExports<SarifValidationSkimmerBase>(
                new Assembly[]
                {
                    validationRuleAssembly
                }).ToList();
        }

        private static bool IsRelevant(string ruleId, string ruleUnderTest)
            => ruleId == ruleUnderTest || ruleId.StartsWith("JSON");
    }
}
