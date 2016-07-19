// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Converters
{
    class ImportanceToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color brushColor = Colors.Black;
            if (value is AnnotatedCodeLocationImportance)
            {
                var importance = (AnnotatedCodeLocationImportance)value;
                if (importance == AnnotatedCodeLocationImportance.Unimportant)
                {
                    brushColor = Colors.Gray;
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
