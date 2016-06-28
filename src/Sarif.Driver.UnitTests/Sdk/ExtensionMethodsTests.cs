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
        [InlineData("We extract initial lines.\n more text", "We extract initial lines.")]
        [InlineData("We extract initial lines.\r more text", "We extract initial lines.")]
        [InlineData("We append periods", "We append periods.")]
        [InlineData("We append periods\nYes we do", "We append periods.")]
        [InlineData("Embedded periods, e.g., .config, does not fool us. Good return.", "Embedded periods, e.g., .config, does not fool us.")]
        // Expected failure cases
        [InlineData("no space after period.cannot return good sentence.", "no space after period.cannot return good sentence.")]
        [InlineData("Misuse of exempli gratis, e.g. as here, fools us.", "Misuse of exempli gratis, e.g.")]
        public void GetFirstSentenceTests(string input, string expected)
        {
            string actual = ExtensionMethods.GetFirstSentence(input);

            actual.Should().Be(expected);
        }
    }
}
