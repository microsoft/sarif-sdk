// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Windows.Media;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Converters;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.Converters.UnitTests
{
    public class ImportanceToBackgroundConverterTests
    {
        [Fact]
        public void ImportanceToBackgroundConverterHandlesUnimportant()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Unimportant, Colors.Transparent);
        }

        [Fact]
        public void ImportanceToBackgroundConverterHandlesImportant()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Important, Colors.Yellow);
        }

        [Fact]
        public void ImportanceToBackgroundConverterHandlesEssential()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Essential, Colors.Yellow);
        }

        private static void VerifyConversion(AnnotatedCodeLocationImportance importance, Color expectedBrushColor)
        {
            var converter = new ImportanceToBackgroundConverter();

            var brush = (SolidColorBrush)converter.Convert(importance, typeof(string), null, CultureInfo.CurrentCulture);

            brush.Color.Should().Be(expectedBrushColor);
        }
    }
}
