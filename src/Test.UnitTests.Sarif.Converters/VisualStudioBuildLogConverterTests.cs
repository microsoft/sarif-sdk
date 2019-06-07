// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Converters
{
    public class VisualStudioBuildLogConverterTests : ConverterTestsBase<VisualStudioBuildLogConverter>
    {
        private readonly VisualStudioBuildLogConverter _converter;

        public VisualStudioBuildLogConverterTests()
        {
            _converter = new VisualStudioBuildLogConverter();
        }

        [Fact]
        public void VisualStudioBuildLogConverterConverter_WhenInputStreamIsNull_Throws()
        {
            Action action = () =>_converter.Convert(null, new ResultLogObjectWriter(), OptionallyEmittedData.None);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void VisualStudioBuildLogConverterConverter_WhenOutputWriterIsNull_Throws()
        {
            Action action = () => _converter.Convert(new MemoryStream(), null, OptionallyEmittedData.None);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}
