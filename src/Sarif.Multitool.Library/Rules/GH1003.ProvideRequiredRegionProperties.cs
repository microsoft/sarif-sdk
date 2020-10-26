﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// Every result must provide a 'region' that specifies its location with line and optional
    /// column information. GitHub Advanced Security code scanning can display the correct
    /// location only for results that provide this information. At minimum, 'region.startLine'
    /// is required. 'region' can also provide 'startColumn', 'endLine', and 'endColumn',
    /// although all of those have reasonable defaults.
    /// </summary>
    public class ProvideRequiredRegionProperties : SarifValidationSkimmerBase
    {
        public ProvideRequiredRegionProperties() : base(
            RuleId.ProvideRequiredRegionProperties,
            RuleResources.GH1003_ProvideRequiredRegionProperties_FullDescription_Text,
            FailureLevel.Error,
            new string[] {
                nameof(RuleResources.GH1003_ProvideRequiredRegionProperties_Error_MissingRegion_Text),
                nameof(RuleResources.GH1003_ProvideRequiredRegionProperties_Error_MissingRegionProperty_Text)
            },
            enabledByDefault: false
            )
        { }

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!(result.Locations?.Any() == true))
            {
                // Rule GH1001.ProvideRequiredLocationProperties will catch this, so don't
                // report it here.
                return;
            }

            for (int i = 0; i < result.Locations.Count; i++)
            {
                Location location = result.Locations[i];
                PhysicalLocation physicalLocation = location.PhysicalLocation;
                if (physicalLocation == null)
                {
                    // Rule GH1001.ProvideRequiredLocationProperties will catch this, so don't
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
                        nameof(RuleResources.GH1003_ProvideRequiredRegionProperties_Error_MissingRegion_Text));
                }
                else if (physicalLocation.Region.StartLine == 0)
                {
                    string regionPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.Region);

                    // {0}: The 'startLine' property is absent. GitHub Advanced Security code
                    // scanning can display the correct location only for results that provide a
                    // 'region' object with line and optional column information. At minimum,
                    // 'region.startLine' is required. 'region' can also provide 'startColumn',
                    // 'endLine', and 'endColumn', although all of those have reasonable defaults.
                    LogResult(
                        regionPointer,
                        nameof(RuleResources.GH1003_ProvideRequiredRegionProperties_Error_MissingRegionProperty_Text));
                }
            }
        }
    }
}
