// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EnquoteDynamicMessageContent : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2015
        /// </summary>
        public override string Id => RuleId.EnquoteDynamicMessageContent;

        /// <summary>
        /// Placing dynamic content in quotes sets it off from the static text, making it easier
        /// to spot. It's especially helpful when the dynamic content is a string that might contain
        /// spaces, and most especially when the string might be empty (and so would be invisible
        /// if it weren't for the quotes). We recommend single quotes for a less cluttered appearance,
        /// even though English usage would require double quotes.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2015_EnquoteDynamicMessageContent_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2015_EnquoteDynamicMessageContent_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        private static readonly Regex s_nonEnquotedDynamicContextRegex = new Regex(@"(^|[^'])\{[0-9]+\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        protected override void Analyze(Tool tool, string toolPointer)
        {
            if (tool.Driver != null)
            {
                AnalyzeToolDriver(tool.Driver, toolPointer.AtProperty(SarifPropertyName.Driver));
            }
        }

        private void AnalyzeToolDriver(ToolComponent toolComponent, string toolDriverPointer)
        {
            if (toolComponent.Rules != null)
            {
                string rulesPointer = toolDriverPointer.AtProperty(SarifPropertyName.Rules);
                for (int i = 0; i < toolComponent.Rules.Count; i++)
                {
                    AnalyzeReportingDescriptor(toolComponent.Rules[i], rulesPointer.AtIndex(i));
                }
            }
        }

        private void AnalyzeReportingDescriptor(ReportingDescriptor rule, string reportingDescriptorPointer)
        {
            if (rule.MessageStrings != null)
            {
                string messageStringsPointer = reportingDescriptorPointer.AtProperty(SarifPropertyName.MessageStrings);
                foreach (KeyValuePair<string, MultiformatMessageString> message in rule.MessageStrings)
                {
                    AnalyzeMessageString(rule.Id, message.Value.Text, message.Key, messageStringsPointer.AtProperty(message.Key));
                }
            }
        }

        private void AnalyzeMessageString(string ruleId, string messageString, string messageKey, string messagePointer)
        {
            if (string.IsNullOrEmpty(messageString))
            {
                return;
            }

            string textPointer = messagePointer.AtProperty(SarifPropertyName.Text);

            if (s_nonEnquotedDynamicContextRegex.IsMatch(messageString))
            {
                // {0}: In rule '{1}', the message with id '{2}' includes dynamic content that is not
                // enclosed in single quotes. Enquoting dynamic content makes it easier to spot, and
                // single quotes give a less cluttered appearance.
                LogResult(
                    textPointer,
                    nameof(RuleResources.SARIF2015_EnquoteDynamicMessageContent_Note_Default_Text),
                    ruleId,
                    messageKey);
            }
        }
    }
}
