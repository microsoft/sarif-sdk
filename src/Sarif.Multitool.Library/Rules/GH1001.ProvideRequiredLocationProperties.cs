// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideRequiredLocationProperties : SarifValidationSkimmerBase
    {
        public ProvideRequiredLocationProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// GH1001
        /// </summary>
        public override string Id => RuleId.ProvideRequiredLocationProperties;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>([RuleKind.Ghas]);

        /// <summary>
        /// Each result location must provide the property 'physicalLocation.artifactLocation.uri'.
        /// GitHub Advanced Security code scanning will not display a result whose location does not
        /// provide the URI of the artifact that was analyzed.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.GH1001_ProvideRequiredLocationProperties_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.GH1001_ProvideRequiredLocationProperties_Error_NoLocationsArray_Text),
            nameof(RuleResources.GH1001_ProvideRequiredLocationProperties_Error_EmptyLocationsArray_Text),
            nameof(RuleResources.GH1001_ProvideRequiredLocationProperties_Error_MissingLocationProperty_Text)
        };

        public override bool EnabledByDefault => false;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.Locations == null)
            {
                // {0}: The 'locations' property is absent. GitHub Advanced Security code scanning
                // will not display a result unless it provides a location that specifies the URI
                // of the artifact that contains the result.
                LogResult(
                    resultPointer,
                    nameof(RuleResources.GH1001_ProvideRequiredLocationProperties_Error_NoLocationsArray_Text),
                    SarifPropertyName.Locations);
                return;
            }

            string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);
            if (!result.Locations.Any())
            {
                // {0}: The 'locations' array is empty. GitHub Advanced Security code scanning will
                // not display a result unless it provides a location that specifies the URI of the
                // artifact that contains the result.
                LogResult(
                    locationsPointer,
                    nameof(RuleResources.GH1001_ProvideRequiredLocationProperties_Error_EmptyLocationsArray_Text));
                return;
            }

            for (int i = 0; i < result.Locations.Count; i++)
            {
                ValidateLocation(result.Locations[i], locationsPointer.AtIndex(i));
            }

            string relatedLocationsPointer = resultPointer.AtProperty(SarifPropertyName.RelatedLocations);
            if (result.RelatedLocations?.Any() == true)
            {
                for (int i = 0; i < result.RelatedLocations.Count; i++)
                {
                    ValidateLocation(result.RelatedLocations[i], relatedLocationsPointer.AtIndex(i));
                }
            }
        }

        private void ValidateLocation(Location location, string locationPointer)
        {
            string missingProperty = null;

            if (location.PhysicalLocation == null)
            {
                missingProperty = SarifPropertyName.PhysicalLocation;
            }
            else if (location.PhysicalLocation.ArtifactLocation == null)
            {
                missingProperty = SarifPropertyName.ArtifactLocation;
                locationPointer = locationPointer.AtProperty(SarifPropertyName.PhysicalLocation);
            }
            else if (location.PhysicalLocation.ArtifactLocation.Uri == null)
            {
                missingProperty = SarifPropertyName.Uri;
                locationPointer = locationPointer.AtProperty(SarifPropertyName.PhysicalLocation).AtProperty(SarifPropertyName.ArtifactLocation);
            }

            if (missingProperty != null)
            {
                // {0}: The '{1}' property is absent. GitHub Advanced Security code scanning will
                // not display a result whose location does not provide the URI of the artifact
                // that contains the result.
                LogResult(
                    locationPointer,
                    nameof(RuleResources.GH1001_ProvideRequiredLocationProperties_Error_MissingLocationProperty_Text),
                    missingProperty);
            }
        }
    }
}
