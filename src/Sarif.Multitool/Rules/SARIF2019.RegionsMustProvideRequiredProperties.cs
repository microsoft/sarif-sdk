// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RegionsMustProvideRequiredProperties : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2019
        /// </summary>
        public override string Id => RuleId.RegionsMustProvideRequiredProperties;

        // Every result must provide a 'region' that specifies its location with line and
        // optional column information. The GitHub Developer Security Portal only displays
        // results that provide this information. At minimum, 'region.startLine' is required.
        // 'region' can also provide 'startColumn', 'endLine', and 'endColumn', although all
        // of those have reasonable defaults.
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2019_RegionsMustProvideRequiredProperties_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2019_RegionsMustProvideRequiredProperties_Error_MissingRegion_Text),
            nameof(RuleResources.SARIF2019_RegionsMustProvideRequiredProperties_Error_MissingRegionProperty_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override bool EnabledByDefault => false;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!result.Locations?.Any() == true)
            {
                // Rule SARIF2017.LocationsMustProvideRequiredProperties will catch this, so don't
                // report it here.
                return;
            }

            for (int i = 0; i < result.Locations.Count; i++)
            {
                Location location = result.Locations[i];
                PhysicalLocation physicalLocation = location.PhysicalLocation;
                if (physicalLocation == null)
                {
                    // Rule SARIF2017.LocationsMustProvideRequiredProperties will catch this, so don't
                    // report it here.
                    continue;
                }

                if (physicalLocation.Region == null)
                {
                    string physicalLocationPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation);

                    // {0}: The 'region' property is absent. The GitHub Developer Security Portal
                    // only displays results that provide a 'region' object with line and optional
                    // column information. At minimum, 'region.startLine' is required. 'region' can
                    // also provide 'startColumn', 'endLine', and 'endColumn', although all of those
                    // have reasonable defaults.
                    LogResult(
                        physicalLocationPointer,
                        nameof(RuleResources.SARIF2019_RegionsMustProvideRequiredProperties_Error_MissingRegion_Text));
                }
                else if (physicalLocation.Region.StartLine == 0)
                {
                    string regionPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.Region);

                    // {0}: The 'startLine' property is absent. The GitHub Developer Security Portal
                    // only displays results that provide a 'region' object with line and optional
                    // column information. At minimum, 'region.startLine' is required. 'region' can
                    // also provide 'startColumn', 'endLine', and 'endColumn', although all of those
                    // have reasonable defaults.
                    LogResult(
                        regionPointer,
                        nameof(RuleResources.SARIF2019_RegionsMustProvideRequiredProperties_Error_MissingRegionProperty_Text));
                }
            }
        }
    }
}
