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
            return node != null ?
                MakeDisplayString(node) :
                string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        private static string MakeDisplayString(CallTreeNode node)
        {
            // Use the following preferences for the CallTreeNode text.
            // 1. CodeFlowLocation.Location.Message.Text
            // 2. CodeFlowLocation.Location.PhysicalLocation.Region.Snippet.Text
            // 3. "Continuing"
            string text = string.Empty;

            CodeFlowLocation codeFlowLocation = node.Location;
            if (codeFlowLocation != null)
            {
                if (!String.IsNullOrWhiteSpace(codeFlowLocation.Location?.Message?.Text))
                {
                    text = codeFlowLocation.Location.Message.Text;
                }
                else if (!String.IsNullOrWhiteSpace(codeFlowLocation.Location?.PhysicalLocation?.Region?.Snippet?.Text))
                {
                    text = codeFlowLocation.Location.PhysicalLocation.Region.Snippet.Text.Trim();
                }
                else
                {
                    text = Resources.ContinuingCallTreeNodeMessage;
                }
            }

            return text;
        }
    }
}
