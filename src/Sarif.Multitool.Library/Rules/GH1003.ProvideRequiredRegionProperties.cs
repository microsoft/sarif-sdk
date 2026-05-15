// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideRequiredRegionProperties : SarifValidationSkimmerBase
    {
        public ProvideRequiredRegionProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// GH1003
        /// </summary>
        public override string Id => RuleId.ProvideRequiredRegionProperties;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Gh });

        // Every result must provide a 'region' that specifies its location with line and optional
        // column information. GitHub Advanced Security code scanning can display the correct
        // location only for results that provide this information. At minimum, 'region.startLine'
        // is required. 'region' can also provide 'startColumn', 'endLine', and 'endColumn',
        // although all of those have reasonable defaults.
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.GH1003_ProvideRequiredRegionProperties_FullDescription_Text };

        protected override ICollection<string> MessageResourceNames => new List<string> {
            nameof(RuleResources.GH1003_ProvideRequiredRegionProperties_Error_MissingRegion_Text),
            nameof(RuleResources.GH1003_ProvideRequiredRegionProperties_Error_MissingRegionProperty_Text)
        };

        public override bool EnabledByDefault => false;

        // See AI1003 for rationale on switching to PhysicalLocation/Region visitors. GH1001
        // (ProvideRequiredLocationProperties) still catches result-level missing-locations cases,
        // so we focus on missing/invalid regions wherever they appear.
        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            if (physicalLocation == null)
            {
                return;
            }

            if (physicalLocation.Region == null)
            {
                // {0}: The 'region' property is absent. GitHub Advanced Security code scanning
                // can display the correct location only for results that provide a 'region'
                // object with line and optional column information. At minimum,
                // 'region.startLine' is required.
                LogResult(
                    physicalLocationPointer,
                    nameof(RuleResources.GH1003_ProvideRequiredRegionProperties_Error_MissingRegion_Text));
            }
        }

        protected override void Analyze(Region region, string regionPointer)
        {
            if (region == null || region.IsBinaryRegion)
            {
                return;
            }

            if (region.StartLine == 0)
            {
                // {0}: The 'startLine' property is absent. GitHub Advanced Security code
                // scanning can display the correct location only for results that provide a
                // 'region' object with line and optional column information. At minimum,
                // 'region.startLine' is required.
                LogResult(
                    regionPointer,
                    nameof(RuleResources.GH1003_ProvideRequiredRegionProperties_Error_MissingRegionProperty_Text));
            }
        }
    }
}
