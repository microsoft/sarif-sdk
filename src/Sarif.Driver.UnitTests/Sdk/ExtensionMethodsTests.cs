// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class ExtensionMethodsTests
    {
        [Theory]
        [InlineData("first (example.dll) sentence. uncapitalized text", "first (example.dll) sentence. uncapitalized text.")]
        [InlineData("first 'example.dll' sentence. uncapitalized text", "first 'example.dll' sentence. uncapitalized text.")]
        [InlineData("first (') sentence. uncapitalized text", "first (') sentence. uncapitalized text.")]
        [InlineData("first '(' sentence. uncapitalized text", "first '(' sentence. uncapitalized text.")]
        [InlineData("first (example.dll) sentence. More text", "first (example.dll) sentence.")]
        [InlineData("first 'example.dll' sentence. More text", "first 'example.dll' sentence.")]
        [InlineData("first (') sentence. More text", "first (') sentence.")]
        [InlineData("first '(' sentence. More text", "first '(' sentence.")]
        [InlineData("We extract initial lines.\n more text", "We extract initial lines.")]
        [InlineData("We extract initial lines.\r more text", "We extract initial lines.")]
        [InlineData("We append periods", "We append periods.")]
        [InlineData("We append periods\nYes we do", "We append periods.")]
        [InlineData("Embedded periods, e.g., .config, does not fool us. Good return.", "Embedded periods, e.g., .config, does not fool us.")]
        [InlineData("Mismatched 'apostrophes', such as in a contraction, don't fool us anymore", "Mismatched 'apostrophes', such as in a contraction, don't fool us anymore.")]
        [InlineData("Misuse of exempli gratis, e.g. as here, no longer fools us.", "Misuse of exempli gratis, e.g. as here, no longer fools us.")]
        [InlineData("Abbreviations such as approx. don't fool us.", "Abbreviations such as approx. don't fool us.")]
        // Expected bad output cases
        [InlineData("no space after period.cannot return good sentence.", "no space after period.cannot return good sentence.")]
        public void GetFirstSentenceTests(string input, string expected)
        {
            string actual = ExtensionMethods.GetFirstSentence(input);
            actual.Should().Be(expected);
        }
    }
}
