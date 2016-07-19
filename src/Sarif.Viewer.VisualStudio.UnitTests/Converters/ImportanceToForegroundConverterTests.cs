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
    public class ImportanceToForegroundConverterTests
    {
        [Fact]
        public void ImportanceToForegroundConverterHandlesUnimportant()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Unimportant, Colors.Gray);
        }

        [Fact]
        public void ImportanceToForegroundConverterHandlesImportant()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Important, Colors.Black);
        }

        [Fact]
        public void ImportanceToForegroundConverterHandlesEssential()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Essential, Colors.Black);
        }

        private static void VerifyConversion(AnnotatedCodeLocationImportance importance, Color expectedBrushColor)
        {
            var converter = new ImportanceToForegroundConverter();

            var brush = (SolidColorBrush)converter.Convert(importance, typeof(string), null, CultureInfo.CurrentCulture);

            brush.Color.Should().Be(expectedBrushColor);
        }
    }
}
