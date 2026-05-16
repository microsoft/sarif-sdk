// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AIProvideRequiredRegionProperties : SarifValidationSkimmerBase
    {
        public AIProvideRequiredRegionProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1003
        /// </summary>
        public override string Id => RuleId.AIProvideRequiredRegionProperties;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1003_ProvideRequiredRegionProperties_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1003_ProvideRequiredRegionProperties_Error_MissingRegion_Text),
            nameof(RuleResources.AI1003_ProvideRequiredRegionProperties_Error_MissingRegionProperty_Text)
        };

        // Walk every physical-location-and-region pair the framework visits — relatedLocations,
        // code flows, fixes, attachments, notification locations, and contextRegions all live
        // under PhysicalLocation, not just result.locations[]. The Result-level hand-walk that
        // this rule used historically silently passed those shapes.
        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            if (physicalLocation == null)
            {
                return;
            }

            if (physicalLocation.Region == null)
            {
                LogResult(
                    physicalLocationPointer,
                    nameof(RuleResources.AI1003_ProvideRequiredRegionProperties_Error_MissingRegion_Text));
            }
        }

        // Region-level visitor catches every Region in the tree (including contextRegions and
        // regions nested inside artifactChanges/attachments/codeFlows/threadFlows). A region
        // without startLine is invalid in the AI profile regardless of where it's anchored.
        protected override void Analyze(Region region, string regionPointer)
        {
            if (region == null || region.IsBinaryRegion)
            {
                return;
            }

            if (region.StartLine == 0)
            {
                LogResult(
                    regionPointer,
                    nameof(RuleResources.AI1003_ProvideRequiredRegionProperties_Error_MissingRegionProperty_Text));
            }
        }
    }
}
