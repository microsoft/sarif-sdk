// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideDynamicMessageContent : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2014
        /// </summary>
        public override string Id => RuleId.ProvideDynamicMessageContent;

        /// <summary>
        /// Including "dynamic content" (information that varies among results from the same rule)
        /// makes your messages more specific.It avoids the "wall of bugs" phenomenon, where hundreds
        /// of occurrences of the same message appear unapproachable.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2014_ProvideDynamicMessageContent_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2014_ProvideDynamicMessageContent_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

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
                    nameof(RuleResources.SARIF2014_ProvideDynamicMessageContent_Note_Default_Text),
                    ruleId,
                    messageKey);
            }
        }
    }
}
