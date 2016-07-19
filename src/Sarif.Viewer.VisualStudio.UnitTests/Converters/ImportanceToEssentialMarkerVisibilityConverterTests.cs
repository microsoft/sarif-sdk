// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Windows;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Converters;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.Converters.UnitTests
{
    public class ImportanceToEssentialMarkerVisibilityConverterTests
    {
        [Fact]
        public void ImportanceToExclamationPointConverterHandlesUnimportant()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Unimportant, Visibility.Collapsed);
        }

        [Fact]
        public void ImportanceToExclamationPointConverterHandlesImportant()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Important, Visibility.Collapsed);
        }

        [Fact]
        public void ImportanceToExclamationPointConverterHandlesEssential()
        {
            VerifyConversion(AnnotatedCodeLocationImportance.Essential, Visibility.Visible);
        }

        private static void VerifyConversion(AnnotatedCodeLocationImportance importance, Visibility expectedVisibility)
        {
            var converter = new ImportanceToEssentialMarkerVisibilityConverter();

            Visibility visibility = (Visibility)converter.Convert(importance, typeof(Visibility), null, CultureInfo.CurrentCulture);

            visibility.Should().Be(expectedVisibility);
        }

    }
}
