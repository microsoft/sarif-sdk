// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Windows;
using System.Windows.Data;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Converters
{
    public class ImportanceToEssentialMarkerVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is AnnotatedCodeLocationImportance)
            {
                var importance = (AnnotatedCodeLocationImportance)value;
                return importance == AnnotatedCodeLocationImportance.Essential
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
