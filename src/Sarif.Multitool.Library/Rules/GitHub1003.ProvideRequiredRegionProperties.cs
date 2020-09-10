// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideRequiredRegionProperties : SarifValidationSkimmerBase
    {
        /// <summary>
        /// GitHub1003
        /// </summary>
        public override string Id => RuleId.ProvideRequiredRegionProperties;

        // Every result must provide a 'region' that specifies its location with line and optional
        // column information. GitHub Advanced Security code scanning can display the correct
        // location only for results that provide this information. At minimum, 'region.startLine'
        // is required. 'region' can also provide 'startColumn', 'endLine', and 'endColumn',
        // although all of those have reasonable defaults.
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.GitHub1003_ProvideRequiredRegionProperties_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.GitHub1003_ProvideRequiredRegionProperties_Error_MissingRegion_Text),
            nameof(RuleResources.GitHub1003_ProvideRequiredRegionProperties_Error_MissingRegionProperty_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override bool EnabledByDefault => false;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!(result.Locations?.Any() == true))
            {
                // Rule GitHub1001.ProvideRequiredLocationProperties will catch this, so don't
                // report it here.
                return;
            }

            for (int i = 0; i < result.Locations.Count; i++)
            {
                Location location = result.Locations[i];
                PhysicalLocation physicalLocation = location.PhysicalLocation;
                if (physicalLocation == null)
                {
                    // Rule GitHub1001.ProvideRequiredLocationProperties will catch this, so don't
                    // report it here.
                    continue;
                }

                if (physicalLocation.Region == null)
                {
                    string physicalLocationPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation);

                    // {0}: The 'region' property is absent. GitHub Advanced Security code scanning
                    // can display the correct location only for results that provide a 'region'
                    // object with line and optional column information. At minimum,
                    // 'region.startLine' is required. 'region' can also provide 'startColumn',
                    // 'endLine', and 'endColumn', although all of those have reasonable defaults.
                    LogResult(
                        physicalLocationPointer,
                        nameof(RuleResources.GitHub1003_ProvideRequiredRegionProperties_Error_MissingRegion_Text));
                }
                else if (physicalLocation.Region.StartLine == 0)
                {
                    string regionPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.Region);

                    // {0}: The 'startLine' property is absent. GitHub
                    // can display the correct location only for results that provide a
                    // 'region' object with line and optional column information. At minimum,
                    // 'region.startLine' is required. 'region' can also provide 'startColumn',
                    // 'endLine', and 'endColumn', although all of those have reasonable defaults.
                    LogResult(
                        regionPointer,
                        nameof(RuleResources.GitHub1003_ProvideRequiredRegionProperties_Error_MissingRegionProperty_Text));
                }
            }
        }
    }
}
