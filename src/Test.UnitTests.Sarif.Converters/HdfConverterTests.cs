// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class HdfConverterTests : ConverterTestsBase<HdfConverter>
    {
        private static readonly string NoOutputExpected = string.Empty;

        [Fact]
        public void Converter_RequiresInputStream()
        {
            var converter = new HdfConverter();
            Action action = () => converter.Convert(input: null, output: new ResultLogObjectWriter(), dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_RequiresResultLogWriter()
        {
            var converter = new HdfConverter();
            Action action = () => converter.Convert(input: new MemoryStream(), output: null, dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_WhenInputIsEmpty_ReturnsNoResults()
        {
            string input = GetResourceText("Inputs.Empty.json");
            string expectedOutput = GetResourceText("ExpectedOutputs.NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenResultRowIsInvalid_ThrowsExpectedException()
        {
            string input = GetResourceText("Inputs.InvalidResult.json");
            Action action = () => RunTestCase(input, NoOutputExpected);
            action.Should().Throw<JsonSerializationException>();
        }

        [Fact]
        public void Converter_WhenInputContainsValidResults_ReturnsExpectedOutput()
        {
            string input = GetResourceText("Inputs.ValidResults.json");
            string expectedOutput = GetResourceText("ExpectedOutputs.ValidResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        private static readonly ResourceExtractor s_extractor = new ResourceExtractor(typeof(HdfConverterTests));
        private const string ResourceNamePrefix = ToolFormat.Hdf;

        private static string GetResourceText(string resourceNameSuffix) =>
            s_extractor.GetResourceText($"TestData.{ResourceNamePrefix}.{resourceNameSuffix}");
    }
}
