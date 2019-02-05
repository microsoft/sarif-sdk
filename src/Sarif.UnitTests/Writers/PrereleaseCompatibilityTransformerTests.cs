// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.TestUtilities;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class PrereleaseCompatibilityTransformerTests : FileDiffingTests, IClassFixture<PrereleaseCompatibilityTransformerTests.PrereleaseCompatibilityTransformerTestsFixture>
    {
        public class PrereleaseCompatibilityTransformerTestsFixture : DeletesOutputsDirectoryOnClassInitializationFixture { }

        public PrereleaseCompatibilityTransformerTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        protected override bool RebaselineExpectedResults => false;

        protected override string ConstructTestOutputFromInputResource(string inputResourceName)
        {
            string inputResourceText = GetResourceText(inputResourceName);

            PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                inputResourceText,
                forceUpdate: false,
                formatting: Formatting.Indented, out string transformedLog);

            return transformedLog;
        }

        [Fact]
        public void PrereleaseCompatibilityTransformer_UpgradesPrereleaseTwoZeroZero()
        {
            string comprehensiveSarifPath = Path.Combine(Environment.CurrentDirectory, @"v2\ObsoleteFormats\ComprehensivePrereleaseTwoZeroZero.sarif");

            string sarifText = File.ReadAllText(comprehensiveSarifPath);

            PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(sarifText, forceUpdate: false, formatting: Formatting.None, out sarifText);

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
    }
}
