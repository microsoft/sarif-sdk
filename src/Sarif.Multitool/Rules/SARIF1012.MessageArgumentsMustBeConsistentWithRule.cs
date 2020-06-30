// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class MessageArgumentsMustBeConsistentWithRule : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF1012
        /// </summary>
        public override string Id => RuleId.MessageArgumentsMustBeConsistentWithRule;

        /// <summary>
        /// Placeholder
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_Error_MessageIdMustExist_Text),
            nameof(RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_Error_SupplyEnoughMessageArguments_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        private static readonly Regex s_replacementSequenceRegex = new Regex(@"\{(<index>\d+)\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private IList<ReportingDescriptor> currentRules;
        private Run run;

        protected override void Analyze(Run run, string runPointer)
        {
            this.run = run;
            this.currentRules = run.Tool.Driver?.Rules;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            // If message.id is present, check that a message with that id exists in the rule.
            if (!string.IsNullOrEmpty(result.Message.Id))
            {
                ReportingDescriptor rule = result.GetRule(this.run);

                if (this.currentRules == null
                    || rule.MessageStrings?.ContainsKey(result.Message.Id) == false)
                {
                    // {0}: Placeholder {1} {2}
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_Error_MessageIdMustExist_Text),
                        result.Message.Id,
                        result.ResolvedRuleId(run) ?? "null");
                    return;
                }

                // A message with the specified key is present in the rule. Check if the result supplied enough arguments.
                string messageText = rule.MessageStrings[result.Message.Id].Text;
                int placeholderMaxPosition = PlaceholderMaxPosition(messageText);
                if (placeholderMaxPosition > (result.Message.Arguments?.Count ?? 0))
                {
                    // {0}: Placeholder {1} {2} {3} {4} {5}
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_Error_SupplyEnoughMessageArguments_Text),
                        result.Message.Arguments.Count.ToString(),
                        result.Message.Id,
                        result.ResolvedRuleId(run) ?? "null",
                        placeholderMaxPosition.ToString(),
                        messageText);
                }
            }
        }

        private int PlaceholderMaxPosition(string text)
        {
            int max = -1;
            foreach (Match match in s_replacementSequenceRegex.Matches(text))
            {
                int index = int.Parse(match.Groups["index"].Value);
                max = Math.Max(max, index);
            }

            return max++;
        }
    }
}
