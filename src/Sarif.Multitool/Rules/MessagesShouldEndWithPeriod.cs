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

        public override ResultLevel DefaultLevel => ResultLevel.Warning;

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

        protected override void Analyze(Rule rule, string rulePointer)
        {
            if (rule.MessageStrings != null)
            {
                foreach (string key in rule.MessageStrings.Keys)
                {
                    string messageString = rule.MessageStrings[key];
                    if (DoesNotEndWithPeriod(messageString))
                    {
                        string messagePointer = rulePointer
                            .AtProperty(SarifPropertyName.MessageStrings)
                            .AtProperty(key);

                        LogResult(
                            messagePointer,
                            nameof(RuleResources.SARIF1008_Default),
                            messageString);
                    }
                }

                foreach (string key in rule.RichMessageStrings.Keys)
                {
                    string messageString = rule.RichMessageStrings[key];
                    if (DoesNotEndWithPeriod(messageString))
                    {
                        string messagePointer = rulePointer
                            .AtProperty(SarifPropertyName.RichMessageStrings)
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
            string messageText = message.Text;
            if (!String.IsNullOrEmpty(messageText) && DoesNotEndWithPeriod(messageText))
            {
                string textPointer = messagePointer.AtProperty(SarifPropertyName.Text);

                LogResult(
                    textPointer,
                    nameof(RuleResources.SARIF1008_Default),
                    messageText);
            }

            string messageRichText = message.RichText;
            if (!String.IsNullOrEmpty(messageRichText) && DoesNotEndWithPeriod(messageRichText))
            {
                string richTextPointer = messagePointer.AtProperty(SarifPropertyName.RichText);

                LogResult(
                    richTextPointer,
                    nameof(RuleResources.SARIF1008_Default),
                    messageRichText);
            }
        }

        private static bool DoesNotEndWithPeriod(string message)
        {
            return message != null && !message.EndsWith(".", StringComparison.Ordinal);
        }
    }
}
