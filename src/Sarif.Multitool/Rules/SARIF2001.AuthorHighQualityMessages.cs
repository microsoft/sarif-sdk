// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AuthorHighQualityMessages : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2001
        /// </summary>
        public override string Id => RuleId.AuthorHighQualityMessages;

        /// <summary>
        /// Follow authoring practices that make your rule messages readable, understandable, and
        /// actionable.
        ///
        /// Including "dynamic content" (information that varies among results from the same rule)
        /// makes your messages more specific.It avoids the "wall of bugs" phenomenon, where hundreds
        /// of occurrences of the same message appear unapproachable.
        ///
        /// Placing dynamic content in quotes sets it off from the static text, making it easier
        /// to spot. It's especially helpful when the dynamic content is a string that might contain
        /// spaces, and most especially when the string might be empty (and so would be invisible
        /// if it weren't for the quotes). We recommend single quotes for a less cluttered appearance,
        /// even though English usage would require double quotes.
        ///
        /// Finally, write in complete sentences and end each sentence with a period.This guidance
        /// does not apply to Markdown messages, which might include formatting that makes the punctuation
        /// unnecessary.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2001_AuthorHighQualityMessages_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2001_AuthorHighQualityMessages_Warning_EnquoteDynamicContent_Text),
            nameof(RuleResources.SARIF2001_AuthorHighQualityMessages_Warning_IncludeDynamicContent_Text),
            nameof(RuleResources.SARIF2001_AuthorHighQualityMessages_Warning_TerminateWithPeriod_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        private static readonly Regex s_dynamicContentRegex = new Regex(@"\{[0-9]+\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
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

            if (!s_dynamicContentRegex.IsMatch(messageString))
            {
                // {0}: In rule '{1}', the message with id '{2}' does not include any dynamic content.
                // Dynamic content makes your messages more specific and avoids the "wall of bugs"
                // phenomenon.
                LogResult(
                    textPointer,
                    nameof(RuleResources.SARIF2001_AuthorHighQualityMessages_Warning_IncludeDynamicContent_Text),
                    ruleId,
                    messageKey);
            }

            if (s_nonEnquotedDynamicContextRegex.IsMatch(messageString))
            {
                // {0}: In rule '{1}', the message with id '{2}' includes dynamic content that is not
                // enclosed in single quotes. Enquoting dynamic content makes it easier to spot, and
                // single quotes give a less cluttered appearance.
                LogResult(
                    textPointer,
                    nameof(RuleResources.SARIF2001_AuthorHighQualityMessages_Warning_EnquoteDynamicContent_Text),
                    ruleId,
                    messageKey);
            }

            if (!messageString.EndsWith(".", StringComparison.Ordinal))
            {
                // {0}: In rule '{1}', the message with id '{2}' does not end in a period. Write rule
                // messages as complete sentences.
                LogResult(
                    textPointer,
                    nameof(RuleResources.SARIF2001_AuthorHighQualityMessages_Warning_TerminateWithPeriod_Text),
                    ruleId,
                    messageKey);
            }
        }
    }
}
