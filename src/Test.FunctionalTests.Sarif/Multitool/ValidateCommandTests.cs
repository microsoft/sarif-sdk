// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Microsoft.CodeAnalysis.Sarif.Visitors;

using Moq;

using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;

using static Microsoft.CodeAnalysis.Sarif.Multitool.Rules.ReviewArraysThatExceedConfigurableDefaults;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ValidateCommandTests : FileDiffingFunctionalTests
    {
        private const string ValidTestFileNameSuffix = "_Valid.sarif";
        private const string InvalidTestFileNameSuffix = "_Invalid.sarif";

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
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF1011_ReferenceFinalSchema_Invalid()
            => RunInvalidTestForRule(RuleId.ReferenceFinalSchema);

        [Fact]
        public void SARIF1012_MessageArgumentsMustBeConsistentWithRule_Valid()
            => RunValidTestForRule(RuleId.MessageArgumentsMustBeConsistentWithRule);

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF1012_MessageArgumentsMustBeConsistentWithRule_Invalid()
            => RunInvalidTestForRule(RuleId.MessageArgumentsMustBeConsistentWithRule);

        [Fact]
        public void SARIF2001_TerminateMessagesWithPeriod_Valid()
            => RunValidTestForRule(RuleId.TerminateMessagesWithPeriod);

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
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
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2004_OptimizeFileSize_Invalid()
            => RunInvalidTestForRule(RuleId.OptimizeFileSize);

        [Fact]
        public void SARIF2005_ProvideToolProperties_Valid()
            => RunValidTestForRule(RuleId.ProvideToolProperties);

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2005_ProvideToolProperties_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideToolProperties);

        [Fact]
        public void SARIF2005_ProvideToolProperties_DottedQuadFileVersion_AcceptedByConfiguration()
        {
            var acceptableVersionProperties = new KeyValuePair<string, object>(
                nameof(ProvideToolProperties.AcceptableVersionProperties),
                new StringSet
                {
                    nameof(ProvideToolProperties.s_dummyToolComponent.SemanticVersion),
                    nameof(ProvideToolProperties.s_dummyToolComponent.DottedQuadFileVersion)
                });

            RunTest(
                inputResourceName: "SARIF2005.ProvideToolProperties_DottedQuadFileVersion.sarif",
                expectedOutputResourceName: "SARIF2005.ProvideToolProperties_DottedQuadFileVersion_Valid.sarif",
                parameter: acceptableVersionProperties);
        }

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2005_ProvideToolProperties_DottedQuadFileVersion_RejectedByConfiguration()
        {
            var acceptableVersionProperties = new KeyValuePair<string, object>(
                nameof(ProvideToolProperties.AcceptableVersionProperties),
                new StringSet
                {
                    nameof(ProvideToolProperties.s_dummyToolComponent.Version),
                    nameof(ProvideToolProperties.s_dummyToolComponent.SemanticVersion)
                });

            RunTest(
                inputResourceName: "SARIF2005.ProvideToolProperties_DottedQuadFileVersion.sarif",
                expectedOutputResourceName: "SARIF2005.ProvideToolProperties_DottedQuadFileVersion_Invalid.sarif",
                parameter: acceptableVersionProperties);
        }

        [Fact]
        public void SARIF2005_ProvideToolProperties_MissingInformationUri_AcceptedByConfiguration()
        {
            var informationUriRequired = new KeyValuePair<string, object>(
                nameof(ProvideToolProperties.InformationUriRequired),
                false);

            RunTest(
                inputResourceName: "SARIF2005.ProvideToolProperties_MissingInformationUri.sarif",
                expectedOutputResourceName: "SARIF2005.ProvideToolProperties_MissingInformationUri_Valid.sarif",
                parameter: informationUriRequired);
        }

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2005_ProvideToolProperties_MissingInformationUri_RejectedByConfiguration()
        {
            var informationUriRequired = new KeyValuePair<string, object>(
               nameof(ProvideToolProperties.InformationUriRequired),
               true);

            RunTest(
                inputResourceName: "SARIF2005.ProvideToolProperties_MissingInformationUri.sarif",
                expectedOutputResourceName: "SARIF2005.ProvideToolProperties_MissingInformationUri_Invalid.sarif",
                parameter: informationUriRequired);
        }

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
        public void SARIF2007_ExpressPathsRelativeToRepoRoot_LoadRelatedUriBaseId_Valid()
            => RunTest("SARIF2007.ExpressPathsRelativeToRepoRoot_LoadRelatedUriBaseId_Valid.sarif");

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2007_ExpressPathsRelativeToRepoRoot_DoNotLoadNotRelatedUriBaseId_Invalid()
            => RunTest("SARIF2007.ExpressPathsRelativeToRepoRoot_DoNotLoadNotRelatedUriBaseId_Invalid.sarif");

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
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
        public void SARIF2012_ProvideRuleProperties_Valid()
            => RunValidTestForRule(RuleId.ProvideRuleProperties);

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2012_ProvideRuleProperties_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideRuleProperties);

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2012_ProvideRuleProperties_WithoutRuleMetadata()
            => RunTest("SARIF2012.ProvideRuleProperties_WithoutRuleMetadata.sarif");

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2012_ProvideRuleProperties_WithoutRules()
            => RunTest("SARIF2012.ProvideRuleProperties_WithoutRules.sarif");

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
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2014_ProvideDynamicMessageContent_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideDynamicMessageContent);

        [Fact]
        public void SARIF2015_EnquoteDynamicMessageContent_Valid()
            => RunValidTestForRule(RuleId.EnquoteDynamicMessageContent);

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2015_EnquoteDynamicMessageContent_Invalid()
            => RunInvalidTestForRule(RuleId.EnquoteDynamicMessageContent);

        [Fact]
        public void SARIF2016_FileUrisShouldBeRelative_Valid()
            => RunValidTestForRule(RuleId.FileUrisShouldBeRelative);

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void SARIF2016_FileUrisShouldBeRelative_Invalid()
            => RunInvalidTestForRule(RuleId.FileUrisShouldBeRelative);

        [Fact]
        public void GH1001_ProvideRequiredLocationProperties_Valid()
            => RunValidTestForRule(RuleId.ProvideRequiredLocationProperties);

        [Fact]
        public void GH1001_ProvideRequiredLocationProperties_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideRequiredLocationProperties);

        [Fact]
        public void GH1002_InlineThreadFlowLocations_Valid()
            => RunValidTestForRule(RuleId.InlineThreadFlowLocations);

        [Fact]
        public void GH1002_InlineThreadFlowLocations_Invalid()
            => RunInvalidTestForRule(RuleId.InlineThreadFlowLocations);

        [Fact]
        public void GH1003_ProvideRequiredRegionProperties_Valid()
            => RunValidTestForRule(RuleId.ProvideRequiredRegionProperties);

        [Fact]
        public void GH1003_ProvideRequiredRegionProperties_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideRequiredRegionProperties);

        [Fact]
        public void GH1004_ReviewArraysThatExceedConfigurableDefaults_Valid()
            => RunArrayLimitTest(ValidTestFileNameSuffix);

        [Fact]
        public void GH1004_ReviewArraysThatExceedConfigurableDefaults_Invalid()
            => RunArrayLimitTest(InvalidTestFileNameSuffix);

        [Fact]
        public void GH1005_LocationsMustBeRelativeUrisOrFilePaths_Valid()
            => RunValidTestForRule(RuleId.LocationsMustBeRelativeUrisOrFilePaths);

        [Fact]
        public void GH1005_LocationsMustBeRelativeUrisOrFilePaths_Invalid()
            => RunInvalidTestForRule(RuleId.LocationsMustBeRelativeUrisOrFilePaths);

        [Fact]
        public void GH1006_ProvideCheckoutPath_Valid()
            => RunValidTestForRule(RuleId.ProvideCheckoutPath);

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void GH1006_ProvideCheckoutPath_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideCheckoutPath);

        [Fact]
        public void GH1007_ProvideFullyFormattedMessageStrings_Valid()
            => RunValidTestForRule(RuleId.ProvideFullyFormattedMessageStrings);

        [Fact]
        public void GH1007_ProvideFullyFormattedMessageStrings_Invalid()
            => RunInvalidTestForRule(RuleId.ProvideFullyFormattedMessageStrings);

        private void RunArrayLimitTest(string testFileNameSuffix)
        {
            // Some of the actual limits are impractically large for testing purposes,
            // so the following test will set smaller values.
            int savedMaxRuns = s_arraySizeLimitDictionary[s_runsPerLogKey];
            int savedMaxRules = s_arraySizeLimitDictionary[s_rulesPerRunKey];
            int savedMaxResults = s_arraySizeLimitDictionary[s_resultsPerRunKey];
            int savedMaxResultLocations = s_arraySizeLimitDictionary[s_locationsPerResultKey];
            int savedMaxCodeFlows = s_arraySizeLimitDictionary[s_codeFlowsPerResultKey];
            int savedMaxThreadFlowLocations = s_arraySizeLimitDictionary[s_locationsPerThreadFlowKey];

            try
            {
                s_arraySizeLimitDictionary[s_runsPerLogKey] = 1;
                s_arraySizeLimitDictionary[s_rulesPerRunKey] = 1;
                s_arraySizeLimitDictionary[s_resultsPerRunKey] = 1;
                s_arraySizeLimitDictionary[s_locationsPerResultKey] = 1;
                s_arraySizeLimitDictionary[s_codeFlowsPerResultKey] = 1;
                s_arraySizeLimitDictionary[s_locationsPerThreadFlowKey] = 1;

                RunTestForRule(RuleId.ReviewArraysThatExceedConfigurableDefaults, testFileNameSuffix);
            }
            finally
            {
                s_arraySizeLimitDictionary[s_runsPerLogKey] = savedMaxRuns;
                s_arraySizeLimitDictionary[s_rulesPerRunKey] = savedMaxRules;
                s_arraySizeLimitDictionary[s_resultsPerRunKey] = savedMaxResults;
                s_arraySizeLimitDictionary[s_locationsPerResultKey] = savedMaxResultLocations;
                s_arraySizeLimitDictionary[s_codeFlowsPerResultKey] = savedMaxCodeFlows;
                s_arraySizeLimitDictionary[s_locationsPerThreadFlowKey] = savedMaxThreadFlowLocations;
            }
        }

        private void RunValidTestForRule(string ruleId)
            => RunTestForRule(ruleId, ValidTestFileNameSuffix);

        private void RunInvalidTestForRule(string ruleId)
            => RunTestForRule(ruleId, InvalidTestFileNameSuffix);

        private void RunTestForRule(string ruleId, string testFileNameSuffix)
        {
            SarifValidationSkimmerBase rule = GetRuleFromId(ruleId);
            string testFileName = MakeTestFileName(rule, testFileNameSuffix);
            RunTest(testFileName);
        }

        private SarifValidationSkimmerBase GetRuleFromId(string ruleId)
            => this.validationRules.Single(vr => vr.Id == ruleId);

        private string MakeTestFileName(ReportingDescriptor rule, string testFileNameSuffix)
            => $"{rule.Id}.{rule.Name}{testFileNameSuffix}";

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            string v2LogText = GetInputSarifTextFromResource(inputResourceName);

            string inputLogDirectory = this.TestOutputDirectory;
            string inputLogFileName = Path.GetFileName(inputResourceName);
            string inputLogFilePath = Path.Combine(this.TestOutputDirectory, inputLogFileName);

            string actualLogFilePath = Guid.NewGuid().ToString();

            // Splits a test file name such as 'GH1001.ProvideRequiredLocationProperties.sarif'
            // and retrieves the rule id, which by convention is a dot-delimited prefix.
            // For this example file, we retrieve 'GH1001'. Note that this file naming 
            // convention allows us to do clever things later in filtering and validating
            // output (because we understand this test file relates specifically to that rule).
            string ruleUnderTest = Path.GetFileNameWithoutExtension(inputLogFilePath).Split('.')[0];

            // All SARIF rule prefixes require update to current release.
            // All rules with JSON prefix are low level syntax/deserialization checks.
            // We can't transform these test inputs as that operation fixes up errors in the file.
            // Also, don't transform the tests for SARIF1011 or SARIF2008, because these rules
            // examine the actual contents of the $schema property.

            string[] shouldNotTransform = { "SARIF1011", "SARIF2008" };

            bool updateInputsToCurrentSarif = IsSarifRule(ruleUnderTest)
                && !shouldNotTransform.Contains(ruleUnderTest);

            var validateOptions = new ValidateOptions
            {
                SarifOutputVersion = SarifVersion.Current,
                TargetFileSpecifiers = new[] { inputLogFilePath },
                OutputFilePath = actualLogFilePath,
                Quiet = true,
                UpdateInputsToCurrentSarif = updateInputsToCurrentSarif,
                OutputFileOptions = new[] { FilePersistenceOptions.PrettyPrint, FilePersistenceOptions.Optimize },
                Level = new List<FailureLevel> { FailureLevel.Error, FailureLevel.Warning, FailureLevel.Note, FailureLevel.None },
                //RuleKindOption = AllRuleKinds
                RuleKindOption = new List<RuleKind>() { RuleKind.Gh, RuleKind.Sarif },
            };

            Mock<IFileSystem> mockFileSystem = MockFactory.MakeMockFileSystem();

            mockFileSystem.Setup(x => x.FileInfoLength(It.IsAny<string>())).Returns(1);
            mockFileSystem.Setup(x => x.DirectoryExists(inputLogDirectory)).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(Array.Empty<string>());
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(inputLogDirectory, inputLogFileName, SearchOption.TopDirectoryOnly)).Returns(new string[] { inputLogFilePath });
            mockFileSystem.Setup(x => x.FileReadAllText(inputLogFilePath)).Returns(v2LogText);
            mockFileSystem.Setup(x => x.FileOpenRead(inputLogFilePath)).Returns(new MemoryStream(Encoding.UTF8.GetBytes(v2LogText)));
            mockFileSystem.Setup(x => x.FileReadAllText(It.IsNotIn<string>(inputLogFilePath))).Returns<string>(path => File.ReadAllText(path));
            mockFileSystem.Setup(x => x.FileExists(inputLogFilePath)).Returns(true);

            // Some rules are disabled by default, so create a configuration file that explicitly
            // enables the rule under test.
            using (TempFile configFile = CreateTempConfigFile(ruleUnderTest, parameter))
            {
                validateOptions.ConfigurationFilePath = configFile.Name;
                mockFileSystem.Setup(x => x.FileExists(validateOptions.ConfigurationFilePath)).Returns(true);

                var validateCommand = new ValidateCommand(mockFileSystem.Object);

                var context = new SarifValidationContext
                {
                    FileSystem = mockFileSystem.Object
                };

                int returnCode = validateCommand.Run(validateOptions, ref context);
                context.ValidateCommandExecution(returnCode);
            }

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

        private static bool IsSarifRule(string ruleId)
            => ruleId.StartsWith("SARIF") || ruleId.StartsWith("GH");

        private TempFile CreateTempConfigFile(string ruleId, object parameter)
        {
            var propertiesDictionary = new PropertiesDictionary();

            if (IsSarifRule(ruleId))
            {
                var rulePropertiesDictionary = new PropertiesDictionary();
                SarifValidationSkimmerBase rule = GetRuleFromId(ruleId);
                RuleEnabledState ruleEnabledState = GetRuleEnabledState(rule);
                rulePropertiesDictionary.Add(nameof(DefaultDriverOptions.RuleEnabled), ruleEnabledState);
                if (parameter is KeyValuePair<string, object> pair)
                {
                    rulePropertiesDictionary.Add(pair.Key, pair.Value);
                }

                propertiesDictionary.Add($"{rule.Moniker}.Options", rulePropertiesDictionary);
            }

            var tempFile = new TempFile(".xml");
            propertiesDictionary.SaveToXml(tempFile.Name);
            return tempFile;
        }

        private static RuleEnabledState GetRuleEnabledState(ReportingDescriptor rule)
        {
            FailureLevel? declaredLevel = rule.DefaultConfiguration?.Level;

            if (declaredLevel.HasValue)
            {
                return declaredLevel.Value switch
                {
                    FailureLevel.Error => RuleEnabledState.Error,
                    FailureLevel.Warning => RuleEnabledState.Warning,
                    FailureLevel.Note => RuleEnabledState.Note,
                    _ => throw new ArgumentException("Non-failure validation rules are not yet supported.", rule.Moniker),
                };
            }
            else
            {
                return RuleEnabledState.Warning;
            }
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
