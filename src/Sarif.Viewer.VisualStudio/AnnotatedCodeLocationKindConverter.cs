// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Sarif.Viewer
{
    public class AnnotatedCodeLocationKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AnnotatedCodeLocationKind kind = (AnnotatedCodeLocationKind)value;

            switch (kind)
            {
                case AnnotatedCodeLocationKind.Unknown: { return "Unknown"; }
                case AnnotatedCodeLocationKind.Stack: { return "Stack(s)"; }
                case AnnotatedCodeLocationKind.ExecutionFlow: { return "Execution Flow(s)"; }
            }
            throw new InvalidOperationException("Unexpected annotated location kind:" + kind.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
