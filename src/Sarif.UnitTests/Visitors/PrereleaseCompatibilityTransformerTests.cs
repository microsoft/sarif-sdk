// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

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

        public PrereleaseCompatibilityTransformerTests(ITestOutputHelper outputHelper) : base (outputHelper){ }

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
        public void PrereleaseCompatibilityTransformer_NestedFiles()
        {
            RunTest("NestedFiles.sarif");
        }
    }
}
