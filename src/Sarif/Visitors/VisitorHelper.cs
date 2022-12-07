// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public static class VisitorHelper
    {
        public static void FlattenMessage(Message node, int ruleIndex, Run run)
        {
            ReportingDescriptor rule = ruleIndex != -1 ? run.Tool.Driver.Rules[ruleIndex] : null;

            if (rule != null &&
                rule.MessageStrings != null &&
                rule.MessageStrings.TryGetValue(node.Id, out MultiformatMessageString formatString))
            {
                node.Text = node.Arguments?.Count > 0
                    ? rule.Format(node.Id, node.Arguments)
                    : formatString?.Text;
            }

            if (node.Text == null &&
                run.Tool.Driver.GlobalMessageStrings?.TryGetValue(node.Id, out formatString) == true)
            {
                node.Text = node.Arguments?.Count > 0 && formatString != null
                    ? string.Format(CultureInfo.CurrentCulture, formatString.Text, node.Arguments.ToArray())
                    : formatString?.Text;
            }
        }
    }
}
