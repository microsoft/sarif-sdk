// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Converters
{
    class ImportanceToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AnnotatedCodeLocationImportance)
            {
                var importance = (AnnotatedCodeLocationImportance)value;
                return importance == AnnotatedCodeLocationImportance.Unimportant ? "Transparent" : "Yellow";
            }
            else
            {
                return "Transparent";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
