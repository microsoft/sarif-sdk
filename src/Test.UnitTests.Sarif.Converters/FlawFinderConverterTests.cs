// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using CsvHelper;
using CsvHelper.TypeConversion;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FlawFinderConverterTests : ConverterTestsBase<FlawFinderConverter>
    {
        private static readonly string NoOutputExpected = string.Empty;

        [Fact]
        public void Converter_RequiresInputStream()
        {
            var converter = new FlawFinderConverter();
            Action action = () => converter.Convert(input: null, output: new ResultLogObjectWriter(), dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_RequiresResultLogWriter()
        {
            var converter = new FlawFinderConverter();
            Action action = () => converter.Convert(input: new MemoryStream(), output: null, dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_WhenInputIsEmpty_ReturnsNoResults()
        {
            string input = Extractor.GetResourceText("Inputs.Empty.csv");
            string expectedOutput = Extractor.GetResourceText("ExpectedOutputs.NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenInputContainsOnlyHeaderLine_ReturnsNoResults()
        {
            string input = Extractor.GetResourceText("Inputs.OnlyHeaderLine.csv");
            string expectedOutput = Extractor.GetResourceText("ExpectedOutputs.NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenHeaderRowIsInvalid_ThrowsExpectedException()
        {
            string input = Extractor.GetResourceText("Inputs.InvalidHeader.csv");
            Action action = () => RunTestCase(input, NoOutputExpected);
            action.Should().Throw<HeaderValidationException>();
        }

        [Fact]
        public void Converter_WhenResultRowIsInvalid_ThrowsExpectedException()
        {
            string input = Extractor.GetResourceText("Inputs.InvalidResult.csv");
            Action action = () => RunTestCase(input, NoOutputExpected);
            action.Should().Throw<TypeConverterException>();
        }

        [Fact]
        public void Converter_WhenInputContainsValidResults_ReturnsExpectedOutput()
        {
            string input = Extractor.GetResourceText("Inputs.ValidResults.csv");
            string expectedOutput = Extractor.GetResourceText("ExpectedOutputs.ValidResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenCsvIsOldVersion_ReturnsExpectedOutput()
        {
            string input = Extractor.GetResourceText("Inputs.OldVersionResult.csv");
            Action action = () => RunTestCase(input, NoOutputExpected);
            action.Should().Throw<HeaderValidationException>();
        }

        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(FlawFinderConverterTests));
        private const string ResourceNamePrefix = ToolFormat.FlawFinder;
    }
}
