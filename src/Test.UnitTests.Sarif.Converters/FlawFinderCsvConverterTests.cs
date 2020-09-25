// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FlawFinderCsvConverterTests : ConverterTestsBase<FlawFinderCsvConverter>
    {
        [Fact]
        public void Converter_RequiresInputStream()
        {
            var mockResultLogWriter = new Mock<IResultLogWriter>();
            var converter = new FlawFinderCsvConverter();

            Action action = () => converter.Convert(input: null, output: mockResultLogWriter.Object, dataToInsert: OptionallyEmittedData.None);

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
        public void Converter_RequiresHeaderRow()
        {
            var mockResultLogWriter = new Mock<IResultLogWriter>();
            var converter = new FlawFinderCsvConverter();

            string input = GetResourceText("Inputs.Empty.csv");
            Action action = () => RunTestCase(input, string.Empty);

            action.Should().Throw<InvalidDataException>().WithMessage(ConverterResources.FlawFinderMissingCsvHeader);
        }

        [Fact]
        public void Converter_RequiresValidHeaderRow()
        {
            var mockResultLogWriter = new Mock<IResultLogWriter>();
            var converter = new FlawFinderCsvConverter();

            string input = GetResourceText("Inputs.InvalidHeader.csv");
            Action action = () => RunTestCase(input, string.Empty);

            action.Should().Throw<InvalidDataException>().WithMessage(ConverterResources.FlawFinderInvalidCsvHeader);
        }

        [Fact]
        public void Converter_HandlesInputWithNoResults()
        {
            throw new NotImplementedException();
        }

        private static readonly ResourceExtractor s_extractor = new ResourceExtractor(typeof(FlawFinderCsvConverterTests));
        private const string ResourceNamePrefix = ToolFormat.FlawFinderCsv;

        private static string GetResourceText(string resourceNameSuffix) =>
            s_extractor.GetResourceText($"TestData.{ResourceNamePrefix}.{resourceNameSuffix}");
    }
}
