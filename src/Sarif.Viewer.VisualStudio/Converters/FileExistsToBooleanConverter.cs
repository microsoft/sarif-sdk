// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Windows.Data;

namespace Microsoft.Sarif.Viewer.Converters
{
    public class FileExistsToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool retVal = false;
            var file = value as string;

            if (!String.IsNullOrWhiteSpace(file))
            {
                if (System.IO.File.Exists(file))
                {
                    retVal = true;
                }
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
