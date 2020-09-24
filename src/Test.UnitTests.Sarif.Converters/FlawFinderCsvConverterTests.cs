// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FlawFinderCsvConverterTests
    {
        [Fact]
        public void Converter_RequiresInputStream()
        {
            var mockResultLogWriter = new Mock<IResultLogWriter>();
            var converter = new FlawFinderCsvConverter();
            Action action = () => converter.Convert(input: null, output: mockResultLogWriter.Object, dataToInsert: OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }
    }
}
