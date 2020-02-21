// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.IO;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class PrereleaseCompatibilityTransformerTests : FileDiffingUnitTests, IClassFixture<PrereleaseCompatibilityTransformerTests.PrereleaseCompatibilityTransformerTestsFixture>
    {
        public class PrereleaseCompatibilityTransformerTestsFixture : DeletesOutputsDirectoryOnClassInitializationFixture { }

        public PrereleaseCompatibilityTransformerTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            string inputResourceText = GetResourceText(inputResourceName);

            PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                inputResourceText,
                formatting: Formatting.Indented,
                out string transformedLog);

            return transformedLog;
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_UpgradesPrereleaseTwoZeroZero()
        {
            string comprehensiveSarifPath = Path.Combine(Environment.CurrentDirectory, @"v2\ObsoleteFormats\ComprehensivePrereleaseTwoZeroZero.sarif");

            string sarifText = File.ReadAllText(comprehensiveSarifPath);

            PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(sarifText, formatting: Formatting.None, out sarifText);

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(sarifText);
            JsonConvert.SerializeObject(sarifLog);
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_NestedFiles()
        {
            RunTest("NestedFiles.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_ComprehensiveFileProperties()
        {
            RunTest("ComprehensiveFileProperties.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_RuleIdCollisions()
        {
            RunTest("RuleIdCollisions.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_RunResources()
        {
            RunTest("RunResources.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_ComprehensiveToolProperties()
        {
            RunTest("ComprehensiveToolProperties.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_ComprehensiveToolProperties_01_24()
        {
            RunTest("ComprehensiveToolProperties.01-24.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_MultiformatMessageStrings()
        {
            RunTest("MultiformatMessageStrings.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_ToolWithLanguage()
        {
            RunTest("ToolWithLanguage.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_NestedInnerExceptionsInNotifications()
        {
            RunTest("NestedInnerExceptionsInNotifications.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_WithExternalPropertyFiles_01_24()
        {
            RunTest("WithExternalPropertyFiles.01-24.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_WithSuppressions()
        {
            RunTest("WithSuppressions.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_ArtifactsWithRoles()
        {
            RunTest("ArtifactsWithRoles.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_OneRunWithRedactionToken()
        {
            RunTest("OneRunWithRedactionToken.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_OneRunWithBasicInvocation()
        {
            RunTest("OneRunWithBasicInvocation.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_OneRunWithInvocationExitCode()
        {
            RunTest("OneRunWithInvocationExitCode.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_LocationWithId()
        {
            RunTest("LocationWithId.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_ThreadFlowLocationWithKind()
        {
            RunTest("ThreadFlowLocationWithKind.sarif");
        }

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1577")]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1584")]
        public void PrereleaseCompatibilityTransformer_PassesRegressionTests()
        {
            RunTest("RegressionTests.sarif");
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_FromSarifV1()
        {
            // We only need one test for the SARIF v1 conversion because for this
            // transformation, the PrereleaseCompatibilityTransformer invokes the
            // SarifVersionOneToCurrentVisitor, which already has an extensive set
            // of tests. Here, we just need to verify that the plumbing from the
            // PrereleaseCompatibilityTransformer to the SarifVersionOneToCurrentVisitor.
            // is in place.
            RunTest("V1.sarif");
        }

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/oasis-tcs/sarif-spec/issues/449")]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1785")]
        public void PrereleaseCompatibilityTransformer_TransformsRtm4Correctly()
        {
            RunTest("ExercisesSchemaRtm5Changes.sarif");
        }
    }
}
