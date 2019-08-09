// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class MSBuildConverterTests :
        FileDiffingUnitTests,
        IClassFixture<MSBuildConverterTests.MSBuildConverterTestsFixture>
    {
        public class MSBuildConverterTestsFixture : DeletesOutputsDirectoryOnClassInitializationFixture { }

        protected override string TestLogResourceNameRoot => "Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Converters.TestData." + TypeUnderTest;

        public MSBuildConverterTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        protected override string ConstructTestOutputFromInputResource(string inputResource)
        {
            byte[] buildLogContents = GetResourceBytes(inputResource);

            var converter = new MSBuildConverter(verbose: false);
            return Utilities.GetConverterJson(converter, buildLogContents);
        }

        [Fact]
        public void VisualStudioBuildLogConverter_ConvertsLogWithNoErrors()
            => RunTest("NoErrors.txt");

        [Fact]
        public void VisualStudioBuildLogConverter_ConvertsLogWithErrorsInVariousFormats()
            => RunTest("SomeErrors.txt");
    }
}
