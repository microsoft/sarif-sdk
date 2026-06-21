// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Taxonomies;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class MapToCweWeaknessNotCategory : SarifValidationSkimmerBase
    {
        public MapToCweWeaknessNotCategory()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1016
        /// </summary>
        public override string Id => RuleId.AIMapToCweWeaknessNotCategory;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1016_MapToCweWeaknessNotCategory_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1016_MapToCweWeaknessNotCategory_Error_Category_Text),
            nameof(RuleResources.AI1016_MapToCweWeaknessNotCategory_Error_NotAWeakness_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            Run run = Context.CurrentRun;
            string ruleId = result.ResolvedRuleId(run);
            if (string.IsNullOrEmpty(ruleId))
            {
                // A missing ruleId is covered by other rules; nothing to classify here.
                return;
            }

            // Only CWE-shaped ids are in scope. The 'NOVEL-' escape hatch and non-CWE
            // taxonomy ids carry no CWE number and are not this rule's concern.
            if (!CweSecuritySeverity.TryGetCweNumber(ruleId, out int cweNumber))
            {
                return;
            }

            // A known Weakness is the only valid mapping target.
            if (CweTaxonomy.IsKnownWeakness(ruleId))
            {
                return;
            }

            string baseCweId = "CWE-" + cweNumber;

            // Sharpen the diagnostic when the id is a recognized Category — the single most
            // common mis-mapping — so the producer learns the class of mistake. Everything
            // else (View, withdrawn entry, typo) gets the general "not a Weakness" message.
            if (CweCategories.TryGetCategoryName(ruleId, out string categoryName))
            {
                LogResult(
                    resultPointer,
                    nameof(RuleResources.AI1016_MapToCweWeaknessNotCategory_Error_Category_Text),
                    ruleId,
                    baseCweId,
                    categoryName);
                return;
            }

            LogResult(
                resultPointer,
                nameof(RuleResources.AI1016_MapToCweWeaknessNotCategory_Error_NotAWeakness_Text),
                ruleId,
                baseCweId);
        }
    }
}
