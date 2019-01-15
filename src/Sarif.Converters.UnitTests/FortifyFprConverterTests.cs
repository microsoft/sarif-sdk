// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FortifyFprConverterTests : FileDiffingTests, IClassFixture<FortifyFprConverterTests.FortifyFprConverterTestsFixture>
    {
        public class FortifyFprConverterTestsFixture : DeletesOutputsDirectoryOnClassInitializationFixture { }

        protected override string TestLogResourceNameRoot => "Microsoft.CodeAnalysis.Sarif.Converters.UnitTests.TestData." + TypeUnderTest;

        public FortifyFprConverterTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        protected override bool RebaselineExpectedResults => false;

        protected override string ConstructTestOutputFromInputResource(string inputResource)
        {
            byte[] fprData = GetResourceBytes(inputResource);

            var converter = new FortifyFprConverter();
            return Utilities.GetConverterJson(converter, fprData);
        }

        [Fact]
        public void FortifyFprConverter_Convert_OneResultBasic()
        {
            RunTest("OneResultBasic.fpr");
        }

        [Fact]
        public void FortifyFprConverter_Convert_OneResultWithTwoTraces()
        {
            RunTest("OneResultWithTwoTraces.fpr");
        }

        [Fact]
        public void FortifyFprConverter_Convert_TwoResultsWithNodeRefs()
        {
            RunTest("TwoResultsWithNodeRefs.fpr");
        }
    }
}
