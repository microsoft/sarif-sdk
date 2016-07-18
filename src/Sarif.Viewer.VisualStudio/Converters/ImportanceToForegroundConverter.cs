using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Microsoft.Sarif.Viewer.Converters
{
    class ImportanceToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var node = value as CallTreeNode;
            if (node != null)
            {
                if (node.Location.Importance == AnnotatedCodeLocationImportance.Unimportant)
                {
                    return Color.Gray;
                }
                else
                {
                    return Color.Black;
                }
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
