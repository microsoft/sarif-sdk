// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Emit;
using Microsoft.CodeAnalysis.Sarif.Taxonomies;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideValidRuleId : SarifValidationSkimmerBase
    {
        public ProvideValidRuleId()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1016
        /// </summary>
        public override string Id => RuleId.AIProvideValidRuleId;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1016_ProvideValidRuleId_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1016_ProvideValidRuleId_Error_Category_Text),
            nameof(RuleResources.AI1016_ProvideValidRuleId_Error_NotAWeakness_Text),
            nameof(RuleResources.AI1016_ProvideValidRuleId_Error_NotCweOrNovel_Text)
        };

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            // Only rule descriptors are in scope. Notification descriptors carry their own id
            // namespace, and taxonomy taxa are never visited through this hook (the AI emit path
            // carries the CWE as the native rule id, with no taxonomy reference at all).
            if (Context?.CurrentReportingDescriptorKind != SarifValidationContext.ReportingDescriptorKind.Rule)
            {
                return;
            }

            string id = reportingDescriptor?.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                // A missing id is Base2012's concern.
                return;
            }

            // The 'NOVEL-' escape hatch is a valid rule id for a finding that maps to no CWE.
            // Sub-id grammar is enforced on result.ruleId by AI1012 and at emit time, not here.
            if (AIRuleIdConvention.IsNovel(id))
            {
                return;
            }

            if (AIRuleIdConvention.TryGetCweId(id, out string cweId))
            {
                // A known Weakness is the only valid CWE mapping target.
                if (CweTaxonomy.IsKnownWeakness(cweId))
                {
                    return;
                }

                // Sharpen the diagnostic when the id is a recognized Category — the single most
                // common mis-mapping — so the producer learns the class of mistake. Every other
                // non-Weakness CWE (a View, a withdrawn entry, a typo) gets the general message.
                if (CweCategories.TryGetCategoryName(cweId, out string categoryName))
                {
                    LogResult(
                        reportingDescriptorPointer,
                        nameof(RuleResources.AI1016_ProvideValidRuleId_Error_Category_Text),
                        id,
                        categoryName);
                    return;
                }

                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.AI1016_ProvideValidRuleId_Error_NotAWeakness_Text),
                    id);
                return;
            }

            // Neither a CWE id nor the NOVEL- escape hatch: not a valid AI rule id at all
            // (e.g. a MITRE ATT&CK technique, an OWASP category, or a tool-private id).
            LogResult(
                reportingDescriptorPointer,
                nameof(RuleResources.AI1016_ProvideValidRuleId_Error_NotCweOrNovel_Text),
                id);
        }
    }
}
