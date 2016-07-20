// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Converters
{
    internal class ImportanceToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.Transparent;
            if (value is AnnotatedCodeLocationImportance)
            {
                var importance = (AnnotatedCodeLocationImportance)value;
                if (importance != AnnotatedCodeLocationImportance.Unimportant)
                {
                    brush = Brushes.Yellow;
                }
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
