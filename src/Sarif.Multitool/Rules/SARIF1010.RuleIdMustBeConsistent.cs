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
        /// Every result must contain at least one of the properties 'ruleId' and 'rule.id'.
        /// If both are present, they must be equal. See the SARIF specification ([§3.27.5]
        /// (https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)).
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
                // {0}: This result contains neither of the properties 'ruleId' or 'rule.id'. The SARIF specification
                // ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643))
                // requires at least one of these properties to be present.
                LogResult(
                    pointer, 
                    nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultMustSpecifyRuleId_Text));
            }
            // if both are present, they must be equal.
            else if (!string.IsNullOrWhiteSpace(result.RuleId)
                && !string.IsNullOrWhiteSpace(result.Rule?.Id)
                && !result.RuleId.Equals(result.Rule?.Id, StringComparison.OrdinalIgnoreCase))
            {
                // {0}: This result contains both the 'ruleId' property '{1}' and the 'rule.id' property
                // {2}', but they are not equal. The SARIF specification ([§3.27.5]
                // (https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643))
                // requires that if both of these properties are present, they must be equal.
                LogResult(
                    pointer, 
                    nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultRuleIdMustBeConsistent_Text), 
                    result.RuleId, 
                    result.Rule?.Id);
            }
        }
    }
}
