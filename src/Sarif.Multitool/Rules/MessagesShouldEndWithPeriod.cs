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

        protected override void Analyze(IRule rule, string rulePointer)
        {
            AnalyzeMessageStrings(rule.MessageStrings, rulePointer, SarifPropertyName.MessageStrings);
            AnalyzeMessageStrings(rule.RichMessageStrings, rulePointer, SarifPropertyName.RichMessageStrings);
        }

        private void AnalyzeMessageStrings(
            IDictionary<string, string> messageStrings,
            string rulePointer,
            string propertyName)
        {
            if (messageStrings != null)
            {
                foreach (string key in messageStrings.Keys)
                {
                    string messageString = messageStrings[key];
                    if (!String.IsNullOrEmpty(messageString) && DoesNotEndWithPeriod(messageString))
                    {
                        string messagePointer = rulePointer
                            .AtProperty(propertyName)
                            .AtProperty(key);

                        LogResult(
                            messagePointer,
                            nameof(RuleResources.SARIF1008_Default),
                            messageString);
                    }
                }
            }
        }

        protected override void Analyze(Message message, string messagePointer)
        {
            AnalyzeMessageString(message.Text, messagePointer, SarifPropertyName.Text);
            AnalyzeMessageString(message.RichText, messagePointer, SarifPropertyName.RichText);
        }

        private void AnalyzeMessageString(
            string messageString,
            string messagePointer,
            string propertyName)
        {
            if (!String.IsNullOrEmpty(messageString) && DoesNotEndWithPeriod(messageString))
            {
                string textPointer = messagePointer.AtProperty(propertyName);

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
