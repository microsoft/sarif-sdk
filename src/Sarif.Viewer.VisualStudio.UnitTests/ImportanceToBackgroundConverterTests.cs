// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Converters;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class ImportanceToBackgroundConverterTests
    {
        [Fact]
        public void ImportanceToBackgroundConverterHandlesUnimportant()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Unimportant, "Transparent");
        }

        [Fact]
        public void ImportanceToBackgroundConverterHandlesImportant()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Important, "Yellow");
        }

        [Fact]
        public void ImportanceToBackgroundConverterHandlesEssential()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Essential, "Yellow");
        }

        private static void VerifyConversion(AnnotatedCodeLocationImportance importance, string expectedColor)
        {
            var converter = new ImportanceToBackgroundConverter();

            string color = (string)converter.Convert(importance, typeof(string), null, CultureInfo.CurrentCulture);

            color.Should().Be(expectedColor);
        }

    }
}
