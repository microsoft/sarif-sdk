// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Windows.Documents;
using FluentAssertions;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class SdkUiUtilitiesTests
    {
        [Fact]
        public void SarifSnapshot_UnescapeBrackets()
        {
            string message = @"The quick \[brown fox\] jumps over the lazy dog.";
            string actual = SdkUiUtilities.UnescapeBrackets(message);
            actual.Should().Be("The quick [brown fox] jumps over the lazy dog.");
        }

        [Fact]
        public void SarifSnapshot_GetMessageEmbeddedLinkInlines_DontCreateLinks()
        {
            string message = @"The quick [brown fox](2) jumps over the lazy dog.";

            var expected = new List<Inline>
            {
                new Run("The quick "),
                new Run("brown fox"),
                new Run(" jumps over the lazy dog.")
            };

            var actual = SdkUiUtilities.GetInlinesForErrorMessage(message);

            actual.Count.Should().Be(expected.Count);

            actual[0].Should().BeOfType(expected[0].GetType());
            (actual[0] as Run).Text.Should().Be((expected[0] as Run).Text);
        }

        [Fact]
        public void SarifSnapshot_GetMessageEmbeddedLinkInlines_ZeroLinks()
        {
            string message = @"The quick brown fox jumps over the lazy dog.";

            var expected = new List<Inline>
            {
                new Run("The quick brown fox jumps over the lazy dog.")
            };

            var actual = SdkUiUtilities.GetInlinesForErrorMessage(message, createHyperlinks: true, data: 1, clickHandler: null);

            actual.Count.Should().Be(expected.Count);

            actual[0].Should().BeOfType(expected[0].GetType());
            (actual[0] as Run).Text.Should().Be((expected[0] as Run).Text);
        }

        [Fact]
        public void SarifSnapshot_GetMessageEmbeddedLinkInlines_ZeroLinksEscapedBrackets()
        {
            string message = @"The quick \[brown fox\] jumps over the lazy dog.";

            var expected = new List<Inline>
            {
                new Run("The quick [brown fox] jumps over the lazy dog.")
            };

            var actual = SdkUiUtilities.GetInlinesForErrorMessage(message, createHyperlinks: true, data: 1, clickHandler: null);

            actual[0].Should().BeOfType(expected[0].GetType());
            (actual[0] as Run).Text.Should().Be((expected[0] as Run).Text);
        }

        [Fact]
        public void SarifSnapshot_GetMessageEmbeddedLinkInlines_OneLink()
        {
            string message = @"The quick [brown fox](1) jumps over the lazy dog.";

            var link = new Hyperlink();
            link.Tag = new Tuple<object, int>(1, 1);
            link.Inlines.Add(new Run("brown fox"));

            var expected = new List<Inline>
            {
                new Run("The quick "),
                link,
                new Run(" jumps over the lazy dog.")
            };

            var actual = SdkUiUtilities.GetInlinesForErrorMessage(message, createHyperlinks: true, data: 1, clickHandler: null);

            actual.Count.Should().Be(expected.Count);

            actual[0].Should().BeOfType(expected[0].GetType());
            (actual[0] as Run).Text.Should().Be((expected[0] as Run).Text);

            actual[1].Should().BeOfType(expected[1].GetType());
            (actual[1] as Hyperlink).Inlines.Count.Should().Be((expected[1] as Hyperlink).Inlines.Count);
            ((actual[1] as Hyperlink).Inlines.FirstInline as Run).Text.Should().Be(((expected[1] as Hyperlink).Inlines.FirstInline as Run).Text);
            Tuple<object, int> tagActual = (actual[1] as Hyperlink).Tag as Tuple<object, int>;
            Tuple<object, int> tagExpected = (actual[1] as Hyperlink).Tag as Tuple<object, int>;
            tagActual.Item1.Should().Be(tagExpected.Item1);
            tagActual.Item2.Should().Be(tagExpected.Item2);

            actual[2].Should().BeOfType(expected[2].GetType());
            (actual[2] as Run).Text.Should().Be((expected[2] as Run).Text);
        }

        [Fact]
        public void SarifSnapshot_GetMessageEmbeddedLinkInlines_TwoLinksAndEndsWithLink()
        {
            string message = @"The quick [brown fox](1) jumps over the [lazy dog](2)";

            var link1 = new Hyperlink();
            link1.Tag = new Tuple<object, int>(1, 1);
            link1.Inlines.Add(new Run("brown fox"));

            var link2 = new Hyperlink();
            link2.Tag = new Tuple<object, int>(1, 2);
            link2.Inlines.Add(new Run("lazy dog"));

            var expected = new List<Inline>
            {
                new Run("The quick "),
                link1,
                new Run(" jumps over the "),
                link2
            };

            var actual = SdkUiUtilities.GetInlinesForErrorMessage(message, createHyperlinks: true, data: 1, clickHandler: null);

            actual.Count.Should().Be(expected.Count);

            actual[0].Should().BeOfType(expected[0].GetType());
            (actual[0] as Run).Text.Should().Be((expected[0] as Run).Text);

            actual[1].Should().BeOfType(expected[1].GetType());
            (actual[1] as Hyperlink).Inlines.Count.Should().Be((expected[1] as Hyperlink).Inlines.Count);
            ((actual[1] as Hyperlink).Inlines.FirstInline as Run).Text.Should().Be(((expected[1] as Hyperlink).Inlines.FirstInline as Run).Text);
            Tuple<object, int> tag1Actual = (actual[1] as Hyperlink).Tag as Tuple<object, int>;
            Tuple<object, int> tag1Expected = (actual[1] as Hyperlink).Tag as Tuple<object, int>;
            tag1Actual.Item1.Should().Be(tag1Expected.Item1);
            tag1Actual.Item2.Should().Be(tag1Expected.Item2);

            actual[2].Should().BeOfType(expected[2].GetType());
            (actual[2] as Run).Text.Should().Be((expected[2] as Run).Text);

            actual[3].Should().BeOfType(expected[3].GetType());
            (actual[3] as Hyperlink).Inlines.Count.Should().Be((expected[3] as Hyperlink).Inlines.Count);
            ((actual[3] as Hyperlink).Inlines.FirstInline as Run).Text.Should().Be(((expected[3] as Hyperlink).Inlines.FirstInline as Run).Text);
            Tuple<object, int> tag2Actual = (actual[3] as Hyperlink).Tag as Tuple<object, int>;
            Tuple<object, int> tag2Expected = (actual[3] as Hyperlink).Tag as Tuple<object, int>;
            tag2Actual.Item1.Should().Be(tag2Expected.Item1);
            tag2Actual.Item2.Should().Be(tag2Expected.Item2);
        }

        [Fact]
        public void SarifSnapshot_GetMessageEmbeddedLinkInlines_OneLinkPlusEscapedBrackets()
        {
            string message = @"The quick [brown fox](1) jumps over the \[lazy dog\].";

            var link = new Hyperlink();
            link.Tag = new Tuple<object, int>(1, 1);
            link.Inlines.Add(new Run("brown fox"));

            var expected = new List<Inline>
            {
                new Run("The quick "),
                link,
                new Run(" jumps over the [lazy dog].")
            };

            var actual = SdkUiUtilities.GetInlinesForErrorMessage(message, createHyperlinks: true, data: 1, clickHandler: null);

            actual.Count.Should().Be(expected.Count);

            actual[0].Should().BeOfType(expected[0].GetType());
            (actual[0] as Run).Text.Should().Be((expected[0] as Run).Text);

            actual[1].Should().BeOfType(expected[1].GetType());
            (actual[1] as Hyperlink).Inlines.Count.Should().Be((expected[1] as Hyperlink).Inlines.Count);
            ((actual[1] as Hyperlink).Inlines.FirstInline as Run).Text.Should().Be(((expected[1] as Hyperlink).Inlines.FirstInline as Run).Text);
            Tuple<object, int> tagActual = (actual[1] as Hyperlink).Tag as Tuple<object, int>;
            Tuple<object, int> tagExpected = (actual[1] as Hyperlink).Tag as Tuple<object, int>;
            tagActual.Item1.Should().Be(tagExpected.Item1);
            tagActual.Item2.Should().Be(tagExpected.Item2);

            actual[2].Should().BeOfType(expected[2].GetType());
            (actual[2] as Run).Text.Should().Be((expected[2] as Run).Text);
        }
    }
}
