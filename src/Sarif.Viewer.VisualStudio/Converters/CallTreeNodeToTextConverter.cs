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
            // Use the following preferences for the CallTreeNode text.
            // 1. AnnotatedCodeLocation.Message
            // 2. AnnotatedCodeLocation.Snippet
            // 3. Callee for calls
            // 4. "Return" for returns
            // 5. AnnotatedCodeLocation.Kind
            string text = string.Empty;

            CodeFlowLocation codeFlowLocation = node.Location;
            if (codeFlowLocation != null)
            {
                if (!String.IsNullOrEmpty(codeFlowLocation.Location?.Message?.Text))
                {
                    text = codeFlowLocation.Location.Message.Text;
                }
                else if (!String.IsNullOrEmpty(codeFlowLocation.Location?.PhysicalLocation?.Region?.Snippet?.Text))
                {
                    text = codeFlowLocation.Location.PhysicalLocation.Region.Snippet.Text.Trim();
                }
                else
                {
                    switch (codeFlowLocation.Kind)
                    {
                        case CodeFlowLocationKind.Call:
                            string callee = codeFlowLocation.Target;
                            text = !string.IsNullOrEmpty(callee) ? callee : Resources.UnknownCalleeMessage;
                            break;

                        case CodeFlowLocationKind.CallReturn:
                            text = Resources.ReturnMessage;
                            break;

                        default:
                            if (codeFlowLocation.Kind != default(CodeFlowLocationKind))
                            {
                                text = codeFlowLocation.Kind.ToString();
                            }
                            break;
                    }
                }
            }

            return text;
        }
    }
}
