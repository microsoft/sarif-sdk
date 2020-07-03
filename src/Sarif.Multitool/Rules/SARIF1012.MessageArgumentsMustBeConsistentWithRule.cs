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
        /// The properties of a result's 'message' property must be consistent with the properties
        /// of the rule that the result refers to.
        ///
        /// When a result's 'message' object uses the 'id' and 'arguments' properties (which, by the
        /// way, is recommended: see SARIF2002.ProvideMessageArguments), it must ensure that the rule
        /// actually defines a message string with that id, and that 'arguments' array has enough
        /// elements to provide values for every replacement sequence in the message specified by 'id'.
        /// For example, if the highest numbered replacement sequence in the specified message string
        /// is '{3}', then the 'arguments' array must contain at least 4 elements.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_Error_MessageIdMustExist_Text),
            nameof(RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_Error_SupplyEnoughMessageArguments_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        private static readonly Regex s_replacementSequenceRegex = new Regex(@"\{(?<index>\d+)\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
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
                    // {0}: This message object refers to the message with id '{1}' in rule '{2}',
                    // but that rule does not define a message with that id. When a tool creates a
                    // result message that uses the 'id' property, it must ensure that the specified
                    // rule actually has a message with that id.
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_Error_MessageIdMustExist_Text),
                        result.Message.Id,
                        result.ResolvedRuleId(run) ?? "null");
                    return;
                }

                // A message with the specified key is present in the rule. Check if the result supplied enough arguments.
                string messageText = rule.MessageStrings[result.Message.Id].Text;
                int numArgsRequired = GetNumArgsRequired(messageText);
                int numArgsPresent = result.Message.Arguments?.Count ?? 0;
                if (numArgsRequired > numArgsPresent)
                {
                    // {0}: The message with id '{1}' in rule '{2}' requires '{3}' arguments, but the
                    // 'arguments' array in this message object has only '{4}' element(s). When a tool
                    // creates a result message that use the 'id' and 'arguments' properties, it must
                    // ensure that the 'arguments' array has enough elements to provide values for every
                    // replacement sequence in the message specified by 'id'. For example, if the highest
                    // numbered replacement sequence in the specified message string is '{{3}}', then
                    // the 'arguments' array must contain 4 elements.
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.SARIF1012_MessageArgumentsMustBeConsistentWithRule_Error_SupplyEnoughMessageArguments_Text),
                        result.Message.Id,
                        result.ResolvedRuleId(run) ?? "null",
                        numArgsRequired.ToString(),
                        result.Message.Arguments.Count.ToString());
                }
            }
        }

        private int GetNumArgsRequired(string text)
        {
            int max = -1;
            foreach (Match match in s_replacementSequenceRegex.Matches(text))
            {
                int index = int.Parse(match.Groups["index"].Value);
                max = Math.Max(max, index);
            }

            return max + 1;
        }
    }
}
