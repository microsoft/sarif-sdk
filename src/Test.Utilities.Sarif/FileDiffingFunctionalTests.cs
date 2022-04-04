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

        // We throw this exception here in order to make it apparent to derived classes that they must override this accessor
        protected abstract string IntermediateTestFolder { get; }

        protected override string TestOutputDirectory =>
            Path.Combine(Path.GetDirectoryName(ThisAssembly.Location), $"FunctionalTestOutput.{TypeUnderTest}");

        protected override string TestLogResourceNameRoot =>
            "Test.FunctionalTests.Sarif.TestData." +
            IntermediateTestFolder + "." +
            TypeUnderTest;
    }
}
