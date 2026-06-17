// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Message
    {
        public bool ShouldSerializeArguments() { return this.Arguments.HasAtLeastOneNonNullValue(); }

        public void Flatten(int ruleIndex, Run run)
        {
            ReportingDescriptor rule = ruleIndex != -1 ? run?.Tool?.Driver?.Rules?[ruleIndex] : null;

            if (rule?.MessageStrings != null &&
                rule.MessageStrings.TryGetValue(this.Id, out MultiformatMessageString formatString))
            {
                this.Text = this.Arguments?.Count > 0 && formatString != null
                    ? string.Format(CultureInfo.CurrentCulture, formatString.Text, this.Arguments.ToArray())
                    : formatString?.Text;
            }

            if (this.Text == null &&
                run?.Tool?.Driver?.GlobalMessageStrings != null &&
                run.Tool.Driver.GlobalMessageStrings.TryGetValue(this.Id, out formatString))
            {
                this.Text = this.Arguments?.Count > 0 && formatString != null
                    ? string.Format(CultureInfo.CurrentCulture, formatString.Text, this.Arguments.ToArray())
                    : formatString?.Text;
            }

            if (this.Text != null)
            {
                this.Text = Regex.Replace(this.Text, "{+", m => m.Value.Length == 1 ? "{{" : m.Value);
                this.Text = Regex.Replace(this.Text, "}+", m => m.Value.Length == 1 ? "}}" : m.Value);
            }
        }
    }
}
