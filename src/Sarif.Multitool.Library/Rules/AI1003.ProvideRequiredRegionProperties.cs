// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

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

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!(result.Locations?.Any() == true))
            {
                return;
            }

            for (int i = 0; i < result.Locations.Count; i++)
            {
                Location location = result.Locations[i];
                PhysicalLocation physicalLocation = location.PhysicalLocation;
                if (physicalLocation == null)
                {
                    continue;
                }

                if (physicalLocation.Region == null)
                {
                    string physicalLocationPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation);

                    LogResult(
                        physicalLocationPointer,
                        nameof(RuleResources.AI1003_ProvideRequiredRegionProperties_Error_MissingRegion_Text));
                }
                else if (physicalLocation.Region.StartLine == 0)
                {
                    string regionPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.Region);

                    LogResult(
                        regionPointer,
                        nameof(RuleResources.AI1003_ProvideRequiredRegionProperties_Error_MissingRegionProperty_Text));
                }
            }
        }
    }
}
