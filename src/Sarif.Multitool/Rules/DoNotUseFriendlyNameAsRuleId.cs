﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DoNotUseFriendlyNameAsRuleId : SarifValidationSkimmerBase
    {
        private Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1001_DoNotUseFriendlyNameAsRuleIdDescription
        };

        public override Message FullDescription => _fullDescription;

        public override ResultLevel DefaultLevel => ResultLevel.Warning;

        /// <summary>
        /// SARIF1001
        /// </summary>
        public override string Id => RuleId.DoNotUseFriendlyNameAsRuleId;

        protected override IEnumerable<string> MessageResourceNames
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF1001_Default)
                };
            }
        }

        protected override void Analyze(Rule rule, string rulePointer)
        {
            if (rule.Id != null &&
                rule.Name != null &&
                rule.Name.Text != null &&
                rule.Id.Equals(rule.Name.Text, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(
                    rulePointer,
                    nameof(RuleResources.SARIF1001_Default),
                    rule.Id);
            }
        }
    }
}
