// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class SemmleQLConverterTests : FileDiffingUnitTests
    {
        public SemmleQLConverterTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void SemmleQLConverter_SimpleCsv()
        {
            RunTest("Simple.csv");
        }

        [Fact]
        public void SemmleQLConvert_EmbeddedLocations()
        {
            RunTest("EmbeddedLocations.csv");
        }
    }
}
