// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class LocationsMustHaveRequiredProperties : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2017
        /// </summary>
        public override string Id => RuleId.LocationsMustHaveRequiredProperties;

        /// <summary>
        /// Each result location must provide the property 'physicalLocation.artifactLocation.uri'.
        /// The GitHub Developer Security Portal will not display a result whose location does not
        /// provide the URI of the artifact that was analyzed.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2017_LocationsMustHaveRequiredProperties_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2017_LocationsMustHaveRequired_Properties_Error_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.Locations == null) { return; }

            string jsonPointer = resultPointer.AtProperty(SarifPropertyName.Locations);
            for (int i = 0; i < result.Locations.Count; i++)
            {
                Location location = result.Locations[i];
                string missingProperty = null;

                if (location.PhysicalLocation == null)
                {
                    missingProperty = SarifPropertyName.PhysicalLocation;
                    jsonPointer = jsonPointer.AtIndex(i);
                }
                else if (location.PhysicalLocation.ArtifactLocation == null)
                {
                    missingProperty = SarifPropertyName.ArtifactLocation;
                    jsonPointer = jsonPointer.AtIndex(i).AtProperty(SarifPropertyName.PhysicalLocation);
                }
                else if (location.PhysicalLocation.ArtifactLocation.Uri == null)
                {
                    missingProperty = SarifPropertyName.Uri;
                    jsonPointer = jsonPointer.AtIndex(i).AtProperty(SarifPropertyName.PhysicalLocation).AtProperty(SarifPropertyName.ArtifactLocation);
                }

                if (missingProperty != null)
                {
                    // {0}: The '{1}' property is absent. The GitHub Developer Security Portal will
                    // not display a result whose location does not provide the URI of the artifact
                    // that was analyzed.
                    LogResult(
                        jsonPointer,
                        RuleResources.SARIF2017_LocationsMustHaveRequired_Properties_Error_Default_Text,
                        missingProperty);
                }
            }
        }
    }
}
