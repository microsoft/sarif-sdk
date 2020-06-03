// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class PreferMessageIdToMessageText : SarifValidationSkimmerBase
    {
        private IList<ReportingDescriptor> currentRules;

        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            Text = RuleResources.SARIF1022_PreferMessageIdToMessageText
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1019
        /// </summary>
        public override string Id => RuleId.PreferMessageIdToMessageText;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1022_RuleMetadataDoeNotIncludeMessageId),
            nameof(RuleResources.SARIF1022_RuleMetadataIsNotConsistentWithArguments),
            nameof(RuleResources.SARIF1022_ResultMessageUsesTextProperty)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            this.currentRules = run?.Tool?.Driver?.Rules;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            // if message.text is not null, we have a problem
            if (!string.IsNullOrEmpty(result.Message.Text))
            {
                LogResult(resultPointer, nameof(RuleResources.SARIF1022_ResultMessageUsesTextProperty), result.Message.Text);
                return;
            }

            // if message.id is not null, we have to check if that exists in rules
            if (!string.IsNullOrEmpty(result.Message.Id))
            {
                ReportingDescriptor rule = this.currentRules.FirstOrDefault(r => r.Id == (result.RuleId ?? result.Rule?.Id));
                if (this.currentRules == null 
                    || this.currentRules.Count == 0 
                    || rule == null 
                    || !rule.MessageStrings.ContainsKey(result.Message.Id))
                {
                    LogResult(resultPointer, nameof(RuleResources.SARIF1022_ResultMessageUsesTextProperty), result.Message.Id, (result.RuleId ?? result.Rule?.Id ?? "null"));
                    return;
                }

                // we have rules, we find it but, the count of parameters are invalid
                string messageText = this.currentRules
                    .First(r => r.Id == (result.RuleId ?? result.Rule.Id))
                    .MessageStrings[result.Message.Id].Text;
                if (!IsPlacehodlerInTextValid(messageText, result.Message.Arguments?.Count ?? 0, out int placeholders))
                {
                    //{0}: The result message contains {1} message arguments for the message string with id '{2}', but the metadata for rule '{3}' defines message string '{2}' to take {4} arguments.
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.SARIF1022_RuleMetadataIsNotConsistentWithArguments),
                        result.Message.Arguments.Count.ToString(),
                        (result.RuleId ?? result.Rule.Id),
                        placeholders.ToString(),
                        messageText);
                    return;
                }
            }
        }

        private bool IsPlacehodlerInTextValid(string text, int arguments, out int placeholders)
        {
            MatchCollection matchCollection = Regex.Matches(text, "{\\d+}");
            placeholders = matchCollection.Count;

            for (int i = 0; i < arguments - 1; i++)
            {
                if (!text.Contains($"{i}"))
                {
                    return false;
                }
            }

            // checking the quantity of matches with quantity of arguments
            if (placeholders != arguments)
            {
                return false;
            }

            // checking if the index of the placeholders are right {0} {2} and 2 arguments is invalid
            int index = 0;
            var matches = new Match[placeholders];
            matchCollection.CopyTo(matches, 0);
            foreach (Match match in matches.OrderBy(q => q.Value))
            {
                if (match.Value == $"{{{index}}}")
                {
                    index++;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
