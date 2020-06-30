// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RuleIdMustBeConsistent : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF1010
        /// </summary>
        public override string Id => RuleId.RuleIdMustBeConsistent;
        
        /// <summary>
        /// Placeholder
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1010_RuleIdMustBeConsistent_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultRuleIdMustBeConsistent_Text),
            nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultMustSpecifyRuleId_Text)
        };
        public override FailureLevel DefaultLevel => FailureLevel.Error;
        
        protected override void Analyze(Result result, string resultPointer)
        {
            AnalyzeRuleId(result, resultPointer);
        }

        private void AnalyzeRuleId(Result result, string pointer)
        {
            // At least one of result.ruleId or result.rule.id must be present
            if (string.IsNullOrWhiteSpace(result.RuleId) && string.IsNullOrWhiteSpace(result.Rule?.Id))
            {
                // {0}: Placeholder
                LogResult(
                    pointer, 
                    nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultMustSpecifyRuleId_Text));
            }
            // if both are present, they must be equal.
            else if (!string.IsNullOrWhiteSpace(result.RuleId)
                && !string.IsNullOrWhiteSpace(result.Rule?.Id)
                && !result.RuleId.Equals(result.Rule?.Id, StringComparison.OrdinalIgnoreCase))
            {
                // {0}: Placeholder '{1}' '{2}' '{3}'
                LogResult(
                    pointer, 
                    nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultRuleIdMustBeConsistent_Text), 
                    result.RuleId, 
                    result.Rule?.Id);
            }
        }
    }
}
