// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Windows.Data;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Converters
{
    public class CallTreeNodeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var node = value as CallTreeNode;
            if (node != null)
            {
                return MakeDisplayString(node);
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        private static string MakeDisplayString(CallTreeNode node)
        {
            string text = string.Empty;

            AnnotatedCodeLocation annotatedLocation = node.Location;
            if (annotatedLocation != null)
            {
                Region region = annotatedLocation.PhysicalLocation?.Region;
                if (region != null)
                {
                    text = $"{region.StartLine}: ";
                }

                string message;
                switch (annotatedLocation.Kind)
                {
                    case AnnotatedCodeLocationKind.Call:
                        string callee = annotatedLocation.Callee;
                        message = !string.IsNullOrEmpty(callee) ? callee : Resources.UnknownCalleMessage;
                        break;

                    case AnnotatedCodeLocationKind.CallReturn:
                        message = Resources.ReturnMessage;
                        break;

                    default:
                        message = annotatedLocation.Kind.ToString();
                        break;
                }

                text += message;
            }

            return text;
        }
    }
}
