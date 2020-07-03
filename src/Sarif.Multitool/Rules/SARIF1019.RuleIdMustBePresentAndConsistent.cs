// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RuleIdMustBePresentAndConsistent : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            Text = RuleResources.SARIF1019_RuleIdMustBePresentAndConsistent
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1019
        /// </summary>
        public override string Id => RuleId.RuleIdMustBePresentAndConsistent;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1019_InconsistentResultRuleId),
            nameof(RuleResources.SARIF1019_MissingResultRuleId)
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
                LogResult(pointer, nameof(RuleResources.SARIF1019_MissingResultRuleId));
            }
            // if both are present, they must be equal.
            else if (!string.IsNullOrWhiteSpace(result.RuleId)
                && !string.IsNullOrWhiteSpace(result.Rule?.Id)
                && !result.RuleId.Equals(result.Rule?.Id, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(pointer, nameof(RuleResources.SARIF1019_InconsistentResultRuleId), result.RuleId, result.Rule?.Id);
            }
        }
    }
}
