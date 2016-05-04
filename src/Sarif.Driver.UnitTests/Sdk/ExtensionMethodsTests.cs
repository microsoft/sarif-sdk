// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class ExtensionMethodsTests
    {
        [Theory]
        [InlineData("first (foo.dll) sentence. more text", "first (foo.dll) sentence.")]
        [InlineData("first 'foo.dll' sentence. more text", "first 'foo.dll' sentence.")]
        [InlineData("first (') sentence. more text", "first (') sentence.")]
        [InlineData("first '(' sentence. more text", "first '(' sentence.")]
        [InlineData("first sentence\n more text", "first sentence")]
        [InlineData("first sentence\r more text", "first sentence")]
        public void GetFirstSentenceTests(string input, string expected)
        {
            string actual = ExtensionMethods.GetFirstSentence(input);

            actual.Should().Be(expected);
        }
    }
}
