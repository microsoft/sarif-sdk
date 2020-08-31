
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideRuleFriendlyName : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2025
        /// </summary>
        public override string Id => RuleId.ProvideRuleFriendlyName;

        // Each analysis rule should provide a "friendly name" in its 'name' property, in addition
        // to the stable, opaque identifier in its 'id' property. This helps users see at a glance
        // the purpose of the analysis rule. For uniformity of experience across all tools that
        // produce SARIF, the friendly name should be a single Pascal identifier, for example,
        // 'ProvideRuleFriendlyName'.
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2025_ProvideRuleFriendlyName_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_FriendlyNameMissing_Text),
            nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_FriendlyNameNotAPascalIdentifier_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        private static readonly Regex s_pascalCaseRegex = new Regex(@"^(\p{Lu}[\p{Ll}\p{Nd}]+)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (string.IsNullOrWhiteSpace(reportingDescriptor.Name))
            {
                // {0}: The rule '{1}' does not provide a "friendly name" in its 'name'
                // property. The friendly name should be a single Pascal identifier, for
                // example, 'ProvideRuleFriendlyName', that helps users see at a glance
                // the purpose of the rule.
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_FriendlyNameMissing_Text),
                    reportingDescriptor.Id);
                return;
            }

            if (!s_pascalCaseRegex.IsMatch(reportingDescriptor.Name))
            {
                // {0}: '{1}' is not a Pascal identifier. For uniformity ofexperience across all tools that
                // produce SARIF, the friendly name should be a single Pascal identifier, for example,
                // 'ProvideRuleFriendlyName'.
                LogResult(
                    reportingDescriptorPointer.AtProperty(SarifPropertyName.Name),
                    nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_FriendlyNameNotAPascalIdentifier_Text),
                    reportingDescriptor.Name);
                return;
            }
        }
    }
}
