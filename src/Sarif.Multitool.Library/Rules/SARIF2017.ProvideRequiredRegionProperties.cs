// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class SARIFProvideRequiredRegionProperties : SarifValidationSkimmerBase
    {
        public SARIFProvideRequiredRegionProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// SARIF2017
        /// </summary>
        public override string Id => RuleId.SARIFProvideRequiredRegionProperties;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Sarif });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF2017_ProvideRequiredRegionProperties_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.SARIF2017_ProvideRequiredRegionProperties_Warning_MissingRegion_Text),
            nameof(RuleResources.SARIF2017_ProvideRequiredRegionProperties_Warning_MissingRegionProperty_Text)
        };

        // See AI1003.ProvideRequiredRegionProperties for visitor rationale: the framework's
        // PhysicalLocation / Region visitors auto-walk every nesting level (relatedLocations,
        // codeFlows, fixes, attachments, contextRegions, notification locations), so we no
        // longer hand-walk only result.Locations[].
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
                    nameof(RuleResources.SARIF2017_ProvideRequiredRegionProperties_Warning_MissingRegion_Text));
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
                LogResult(
                    regionPointer,
                    nameof(RuleResources.SARIF2017_ProvideRequiredRegionProperties_Warning_MissingRegionProperty_Text));
            }
        }
    }
}
