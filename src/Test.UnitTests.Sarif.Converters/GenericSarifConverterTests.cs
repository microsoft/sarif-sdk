// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;
using FluentAssertions;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class GenericSarifConverterTests : ConverterTestsBase<GenericSarifConverter>
    {
        [Fact]
        public void Converter_RequiresInputStream()
        {
            var converter = new GenericSarifConverter();
            Action action = () => converter.Convert(input: null, output: new ResultLogObjectWriter(), dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_RequiresResultLogWriter()
        {
            var converter = new GenericSarifConverter();
            Action action = () => converter.Convert(input: new MemoryStream(), output: null, dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_WhenInputIsEmpty_ReturnsNoResults()
        {
            string input = Extractor.GetResourceInputText("NoResults.GenericSarif.sarif");
            string expectedOutput = Extractor.GetResourceExpectedOutputsText("NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenInputContainingNoHelp_ReturnsNoResults()
        {
            string input = Extractor.GetResourceInputText("NoHelp.GenericSarif.sarif");
            string expectedOutput = Extractor.GetResourceExpectedOutputsText("NoHelp.sarif");
            RunTestCase(input, expectedOutput);
        }
        
        [Fact]
        public void Converter_WhenInputContainsValidResults_ReturnsExpectedOutput()
        {
            string input = Extractor.GetResourceInputText("ValidResults.GenericSarif.sarif");
            string expectedOutput = Extractor.GetResourceExpectedOutputsText("ValidResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenInputContainsHelpWithoutText_ReturnsExpectedOutput()
        {
            string input = Extractor.GetResourceInputText("HelpNoText.GenericSarif.sarif");
            string expectedOutput = Extractor.GetResourceExpectedOutputsText("HelpNoText.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenInputContainsHelpWithoutMarkdown_ReturnsExpectedOutput()
        {
            string input = Extractor.GetResourceInputText("HelpNoMarkdown.GenericSarif.sarif");
            string expectedOutput = Extractor.GetResourceExpectedOutputsText("HelpNoMarkdown.sarif");
            RunTestCase(input, expectedOutput);
        }

        private static readonly TestAssetResourceExtractor Extractor = new TestAssetResourceExtractor(typeof(GenericSarifConverterTests));
        private const string ResourceNamePrefix = ToolFormat.GenericSarif;
    }
}