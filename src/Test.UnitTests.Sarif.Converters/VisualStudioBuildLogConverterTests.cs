// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class VisualStudioBuildLogConverterTests : ConverterTestsBase<VisualStudioBuildLogConverter>
    {
        private readonly VisualStudioBuildLogConverter _converter;

        public VisualStudioBuildLogConverterTests()
        {
            _converter = new VisualStudioBuildLogConverter();
        }

        [Fact]
        public void VisualStudioBuildLogConverter_WhenInputStreamIsNull_Throws()
        {
            Action action = () => _converter.Convert(null, new ResultLogObjectWriter(), OptionallyEmittedData.None);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void VisualStudioBuildLogConverter_WhenOutputWriterIsNull_Throws()
        {
            Action action = () => _converter.Convert(new MemoryStream(), null, OptionallyEmittedData.None);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void VisualStudioBuildLogConverter_WhenLogIsEmpty_ProducesRunWithNoResults()
        {
            string buildLog = string.Empty;
            string actualJson = Utilities.GetConverterJson(_converter, buildLog);
            actualJson.Should().BeCrossPlatformEquivalent<SarifLog>(EmptyResultLogText);
        }
    }
}
