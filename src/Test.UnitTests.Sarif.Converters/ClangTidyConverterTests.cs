// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

using YamlDotNet.Core;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class ClangTidyConverterTests : ConverterTestsBase<ClangTidyConverter>
    {
        private static readonly string NoOutputExpected = string.Empty;

        [Fact]
        public void Converter_RequiresInputStream()
        {
            var converter = new ClangTidyConverter();
            Action action = () => converter.Convert(input: null, output: new ResultLogObjectWriter(), dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_RequiresResultLogWriter()
        {
            var converter = new ClangTidyConverter();
            Action action = () => converter.Convert(input: new MemoryStream(), output: null, dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Converter_WhenInputIsEmpty_ReturnsNoResults()
        {
            string input = s_extractor.GetResourceInputText("Empty.yaml");
            string expectedOutput = s_extractor.GetResourceExpectedOutputsText("NoResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        [Fact]
        public void Converter_WhenResultRowIsInvalid_ThrowsExpectedException()
        {
            string input = s_extractor.GetResourceInputText("InvalidResult.yaml");
            Action action = () => RunTestCase(input, NoOutputExpected);
            action.Should().Throw<YamlException>();
        }

        [Fact]
        public void Converter_WhenInputContainsValidResults_ReturnsExpectedOutput()
        {
            string input = s_extractor.GetResourceInputText("ValidResults.yaml");
            string expectedOutput = s_extractor.GetResourceExpectedOutputsText("ValidResults.sarif");
            RunTestCase(input, expectedOutput);
        }

        private static readonly TestAssetResourceExtractor s_extractor = new TestAssetResourceExtractor(typeof(ClangTidyConverterTests));
        private const string ResourceNamePrefix = ToolFormat.ClangTidy;
    }
}
