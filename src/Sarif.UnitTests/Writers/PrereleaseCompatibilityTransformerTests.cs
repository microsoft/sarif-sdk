// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.TestUtilities;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class PrereleaseCompatibilityTransformerTests : FileDiffingTests, IClassFixture<PrereleaseCompatibilityTransformerTests.PrereleaseCompatibilityTransformerTestsFixture>
    {
        public class PrereleaseCompatibilityTransformerTestsFixture : DeletesOutputsDirectoryOnClassInitializationFixture { }

        public PrereleaseCompatibilityTransformerTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        protected override string ConstructTestOutputFromInputResource(string inputResourceName)
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
        public void PrereleaseCompatibilityTransformer_WithReportingDescriptors_01_24()
        {
            RunTest("WithReportingDescriptors.01-24.sarif");
        }


    }
}
