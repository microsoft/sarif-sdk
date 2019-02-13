// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class MessagesShouldEndWithPeriod : SarifValidationSkimmerBase
    {
        private readonly Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1008_MessagesShouldEndWithPeriod
        };

        public override Message FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        /// <summary>
        /// SARIF1008
        /// </summary>
        public override string Id => RuleId.MessagesShouldEndWithPeriod;

        protected override IEnumerable<string> MessageResourceNames
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF1008_Default)
                };
            }
        }

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            AnalyzeMessageStrings(reportingDescriptor.MessageStrings, reportingDescriptorPointer, SarifPropertyName.MessageStrings);
        }

        private void AnalyzeMessageStrings(
            IDictionary<string, MultiformatMessageString> messageStrings,
            string rulePointer,
            string propertyName)
        {
            if (messageStrings != null)
            {
                foreach (string key in messageStrings.Keys)
                {
                    MultiformatMessageString messageString = messageStrings[key];

                    AnalyzeMessageString(messageString.Text, rulePointer, key, SarifPropertyName.Text);
                    AnalyzeMessageString(messageString.Markdown, rulePointer, key, SarifPropertyName.Markdown);
                }
            }
        }

        protected override void Analyze(Message message, string messagePointer)
        {
            AnalyzeMessageString(message.Text, messagePointer, SarifPropertyName.Text);
            AnalyzeMessageString(message.Markdown, messagePointer, SarifPropertyName.Markdown);
        }

        private void AnalyzeMessageString(
            string messageString,
            string messagePointer,
            string propertyName,
            string additionalPropertyName = "")
        {
            if (!String.IsNullOrEmpty(messageString) && DoesNotEndWithPeriod(messageString))
            {
                string textPointer = messagePointer.AtProperty(propertyName).AtProperty(additionalPropertyName);

                LogResult(
                    textPointer,
                    nameof(RuleResources.SARIF1008_Default),
                    messageString);
            }
        }

        private static bool DoesNotEndWithPeriod(string message)
        {
            return message != null && !message.EndsWith(".", StringComparison.Ordinal);
        }
    }
}
