// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RuleIdMustBeConsistent : SarifValidationSkimmerBase
    {
        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1010_RuleIdMustBeConsistent_FullDescription_Text
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override string Id => RuleId.RuleIdMustBeConsistent;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultRuleIdMustBeConsistent_Text),
            nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultMustSpecifyRuleId_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            AnalyzeRuleId(result, resultPointer);
        }

        private void AnalyzeRuleId(Result result, string pointer)
        {
            // At least one of result.ruleId or result.rule.id must be present
            if (string.IsNullOrWhiteSpace(result.RuleId) && string.IsNullOrWhiteSpace(result.Rule?.Id))
            {
                LogResult(pointer, nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultMustSpecifyRuleId_Text));
            }
            // if both are present, they must be equal.
            else if (!string.IsNullOrWhiteSpace(result.RuleId)
                && !string.IsNullOrWhiteSpace(result.Rule?.Id)
                && !result.RuleId.Equals(result.Rule?.Id, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(pointer, nameof(RuleResources.SARIF1010_RuleIdMustBeConsistent_Error_ResultRuleIdMustBeConsistent_Text), result.RuleId, result.Rule?.Id);
            }
        }
    }
}
