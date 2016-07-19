// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Converters
{
    class ImportanceToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color brushColor = Colors.Transparent;
            if (value is AnnotatedCodeLocationImportance)
            {
                var importance = (AnnotatedCodeLocationImportance)value;
                if (importance != AnnotatedCodeLocationImportance.Unimportant)
                {
                    brushColor = Colors.Yellow;
                }
            }

            return new SolidColorBrush(brushColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
