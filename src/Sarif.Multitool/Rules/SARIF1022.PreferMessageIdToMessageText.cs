// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class PreferMessageIdToMessageText : SarifValidationSkimmerBase
    {
        private IList<ReportingDescriptor> currentRules;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1022_PreferMessageIdToMessageText
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1019
        /// </summary>
        public override string Id => RuleId.PreferMessageIdToMessageText;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1022_IncorrectResultMessageArgumentsCount),
            nameof(RuleResources.SARIF1022_NonConsecutiveMessageStringPlaceholders),
            nameof(RuleResources.SARIF1022_ResultMessageUsesTextProperty),
            nameof(RuleResources.SARIF1022_RuleMetadataDoeNotIncludeMessageId)
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
                ReportingDescriptor rule = this.currentRules?.FirstOrDefault(r => r.Id == (result.RuleId ?? result.Rule?.Id));
                if (this.currentRules == null
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
                if (!CheckCountArguments(messageText, result.Message.Arguments?.Count ?? 0, out int placeholdersCount))
                {
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.SARIF1022_IncorrectResultMessageArgumentsCount),
                        result.Message.Arguments.Count.ToString(),
                        result.RuleId ?? result.Rule?.Id,
                        placeholdersCount.ToString(),
                        messageText);
                    return;
                }

                // we have rules, we find it but, we couldnt find the index
                if (!CheckNonConsecutivePlaceholders(messageText, result.Message.Arguments?.Count ?? 0, out int index))
                {
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.SARIF1022_NonConsecutiveMessageStringPlaceholders),
                        messageText,
                        result.RuleId ?? result.Rule?.Id,
                        result.Message.Arguments.Count.ToString(),
                        index.ToString());
                    return;
                }
            }
        }

        private bool CheckCountArguments(string text, int arguments, out int placeholdersCount)
        {
            MatchCollection matchCollection = Regex.Matches(text, "{\\d+}");
            Dictionary<string, Match> matches = new Dictionary<string, Match>();
            foreach (Match match in matchCollection)
            {
                if (!matches.ContainsKey(match.Value))
                {
                    matches.Add(match.Value, match);
                }
            }

            placeholdersCount = matches.Count;
            return arguments == placeholdersCount;
        }

        private bool CheckNonConsecutivePlaceholders(string text, int arguments, out int index)
        {
            index = -1;
            for (int i = 0; i < arguments - 1; i++)
            {
                if (!text.Contains($"{i}"))
                {
                    index = i;
                    return false;
                }
            }

            return true;
        }
    }
}
