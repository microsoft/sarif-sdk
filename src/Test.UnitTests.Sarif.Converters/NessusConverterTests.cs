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
    public class NessusConverterTests : ConverterTestsBase<NessusConverter>
    {
        [Fact]
        public void Converter_RequiresInputStream()
        {
            var converter = new NessusConverter();
            Action action = () => converter.Convert(input: null, output: new ResultLogObjectWriter(), dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_RequiresResultLogWriter()
        {
            var converter = new NessusConverter();
            Action action = () => converter.Convert(input: new MemoryStream(), output: null, dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_WhenInputIsEmpty_ReturnsNoResults()
        {
            string input = Extractor.GetResourceInputText("NoResults.nessus.xml");
            string expectedOutput = Extractor.GetResourceExpectedOutputsText("NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenResultRowIsInvalid_ReturnsEmptyResults()
        {
            string input = Extractor.GetResourceInputText("InvalidResults.nessus.xml");
            string expectedOutput = Extractor.GetResourceExpectedOutputsText("InvalidResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenInputContainsValidResults_ReturnsExpectedOutput()
        {
            string input = Extractor.GetResourceInputText("ValidResults.nessus.xml");
            string expectedOutput = Extractor.GetResourceExpectedOutputsText("ValidResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        private static readonly TestAssetResourceExtractor Extractor = new TestAssetResourceExtractor(typeof(NessusConverterTests));
        private const string ResourceNamePrefix = ToolFormat.Nessus;
    }
}
