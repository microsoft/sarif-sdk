// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class UriConverterTests
    {
        [Theory]
        [InlineData("/relative/path", "/relative/path")]
        [InlineData("standalone", "standalone")]
        [InlineData("relative/path/", "relative/path/")]
        [InlineData("http://abc", "http://abc/")]
        [InlineData(@"file:///c:/space space", "file:///c:/space%20space")]
        public void UriConverter_UriToString(string input, string expected)
        {
            string actual = UriConverter.UriToString(new Uri(input, UriKind.RelativeOrAbsolute));
            actual.Should().Be(expected);
        }
    }
}