// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
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
            VerifyConversion(AnnotatedCodeLocationImportance.Unimportant, "Gray");
        }

        [Fact]
        public void ImportanceToForegroundConverterHandlesImportant()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Important, "Black");
        }

        [Fact]
        public void ImportanceToForegroundConverterHandlesEssential()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Essential, "Black");
        }

        private static void VerifyConversion(AnnotatedCodeLocationImportance importance, string expectedColor)
        {
            var converter = new ImportanceToForegroundConverter();

            string color = (string)converter.Convert(importance, typeof(string), null, CultureInfo.CurrentCulture);

            color.Should().Be(expectedColor);
        }
    }
}
