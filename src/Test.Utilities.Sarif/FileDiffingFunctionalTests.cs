// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public abstract class FileDiffingFunctionalTests : FileDiffingUnitTests
    {
        public FileDiffingFunctionalTests(ITestOutputHelper outputHelper, bool testProducesSarifCurrentVersion = true) :
            base(outputHelper, testProducesSarifCurrentVersion)
        {
        }

        protected abstract string IntermediateTestFolder { get; }

        protected override string TestOutputDirectory =>
            Path.Combine(Path.GetDirectoryName(ThisAssembly.Location), $"FunctionalTestOutput.{TypeUnderTest}");

        protected override string TestBinaryTestDataDirectory =>
            Path.Combine(ProductRootDirectory, "src", TestBinaryName, "TestData", IntermediateTestFolder);

        protected override string TestLogResourceNameRoot =>
            "Test.FunctionalTests.Sarif.TestData." +
            IntermediateTestFolder + "." +
            TypeUnderTest;
    }
}
