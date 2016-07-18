using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Converters
{
    class ImportanceToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var node = value as CallTreeNode;
            if (node != null)
            {
                return node.Location.Importance == AnnotatedCodeLocationImportance.Unimportant
                    ? Color.Gray
                    : Color.Black;
            }
            else
            {
                return Color.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
