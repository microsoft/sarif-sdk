// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class MSBuildConverterTests : ConverterTestsBase<MSBuildConverter>
    {
        [Fact]
        public void Converter_RequiresInputStream()
        {
            var converter = new MSBuildConverter();
            Action action = () => converter.Convert(input: null, output: new ResultLogObjectWriter(), dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_RequiresResultLogWriter()
        {
            var converter = new MSBuildConverter();
            Action action = () => converter.Convert(input: new MemoryStream(), output: null, dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_WhenInputIsEmpty_ReturnsNoResults()
        {
            string input = s_extractor.GetResourceInputText("Empty.ERR");
            string expectedOutput = s_extractor.GetResourceExpectedOutputsText("NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenResultRowIsInvalid_ReturnsNoResults()
        {
            string input = s_extractor.GetResourceInputText("InvalidResult.ERR");
            string expectedOutput = s_extractor.GetResourceExpectedOutputsText("NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenInputContainsValidResults_ReturnsExpectedOutput()
        {
            string input = s_extractor.GetResourceInputText("ValidResults.ERR");
            string expectedOutput = s_extractor.GetResourceExpectedOutputsText("ValidResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        private static readonly TestAssetResourceExtractor s_extractor = new TestAssetResourceExtractor(typeof(MSBuildConverterTests));
    }
}
