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
    public class FlawFinderCsvConverterTests : ConverterTestsBase<FlawFinderCsvConverter>
    {
        private static readonly string NoOutputExpected = string.Empty;

        [Fact]
        public void Converter_RequiresInputStream()
        {
            var converter = new FlawFinderCsvConverter();
            Action action = () => converter.Convert(input: null, output: new ResultLogObjectWriter(), dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_RequiresResultLogWriter()
        {
            var converter = new FlawFinderCsvConverter();
            Action action = () => converter.Convert(input: new MemoryStream(), output: null, dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_WhenInputIsEmpty_ReturnsNoResults()
        {
            string input = GetResourceText("Inputs.Empty.csv");
            string expectedOutput = GetResourceText("ExpectedOutputs.NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenInputContainsOnlyHeaderLine_ReturnsNoResults()
        {
            string input = GetResourceText("Inputs.OnlyHeaderLine.csv");
            string expectedOutput = GetResourceText("ExpectedOutputs.NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenHeaderRowIsInvalid_ThrowsExpectedException()
        {
            string input = GetResourceText("Inputs.InvalidHeader.csv");
            Action action = () => RunTestCase(input, NoOutputExpected);
            action.Should().Throw<HeaderValidationException>();
        }

        [Fact]
        public void Converter_WhenResultRowIsInvalid_ThrowsExpectedException()
        {
            string input = GetResourceText("Inputs.InvalidResult.csv");
            Action action = () => RunTestCase(input, NoOutputExpected);
            action.Should().Throw<TypeConverterException>();
        }

        [Fact]
        public void Converter_WhenInputContainsValidResults_ReturnsExpectedOutput()
        {
            string input = GetResourceText("Inputs.ValidResults.csv");
            string expectedOutput = GetResourceText("ExpectedOutputs.ValidResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        private static readonly ResourceExtractor s_extractor = new ResourceExtractor(typeof(FlawFinderCsvConverterTests));
        private const string ResourceNamePrefix = ToolFormat.FlawFinder;

        private static string GetResourceText(string resourceNameSuffix) =>
            s_extractor.GetResourceText($"TestData.{ResourceNamePrefix}.{resourceNameSuffix}");
    }
}
