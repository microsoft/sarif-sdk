// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DoNotUseFriendlyNameAsRuleId : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF001_DoNotUseFriendlyNameAsRuleIdDescription;

        public override ResultLevel DefaultLevel => ResultLevel.Warning;

        /// <summary>
        /// SARIF001
        /// </summary>
        public override string Id => RuleId.DoNotUseFriendlyNameAsRuleId;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF001_Default)
                };
            }
        }

        protected override void Analyze(Rule rule, string rulePointer)
        {
            if (rule.Id != null &&
                rule.Name != null && 
                rule.Id.Equals(rule.Name, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(
                    rulePointer,
                    nameof(RuleResources.SARIF001_Default),
                    rule.Id);
            }
        }
    }
}
