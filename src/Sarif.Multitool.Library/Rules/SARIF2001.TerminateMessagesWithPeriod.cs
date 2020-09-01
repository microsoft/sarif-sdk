// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class TerminateMessagesWithPeriod : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2001
        /// </summary>
        public override string Id => RuleId.TerminateMessagesWithPeriod;

        /// <summary>
        /// Express plain text result messages as complete sentences and end each sentence with a period.
        /// This guidance does not apply to Markdown messages, which might include formatting that makes
        /// the punctuation unnecessary.
        ///
        /// This is part of a set of authoring practices that make your rule messages more readable, 
        /// understandable, and actionable.See also `SARIF2014.ProvideDynamicMessageContent` and 
        /// `SARIF2015.EnquoteDynamicMessageContent`.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2001_TerminateMessagesWithPeriod_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2001_TerminateMessagesWithPeriod_Warning_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

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

            if (!messageString.EndsWith(".", StringComparison.Ordinal))
            {
                // {0}: In rule '{1}', the message with id '{2}' does not end in a period. Express plain
                // text rule messages as complete sentences. This guidance does not apply to Markdown
                // messages, which might include formatting that makes the punctuation unnecessary.
                LogResult(
                    textPointer,
                    nameof(RuleResources.SARIF2001_TerminateMessagesWithPeriod_Warning_Default_Text),
                    ruleId,
                    messageKey);
            }
        }
    }
}
