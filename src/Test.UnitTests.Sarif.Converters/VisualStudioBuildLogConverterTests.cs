// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Converters;
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
        public void VisualStudioBuildLogConverter_Exists()
        {
            _converter.Should().NotBeNull();
        }
    }
}
