// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class LocationsMustBeRelativeUrisOrFilePaths : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2021
        /// </summary>
        public override string Id => RuleId.LocationsMustBeRelativeUrisOrFilePaths;

        // The GitHub Developer Security Portal only displays results whose locations are specified
        // by file paths, either as relative URIs or as absolute URIs that use the 'file' scheme.
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2021_LocationsMustBeRelativeUrisOrFilePaths_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2021_LocationsMustBeRelativeUrisOrFilePaths_Error_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override bool EnabledByDefault => false;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!(result.Locations?.Any() == true))
            {
                // Rule SARIF2017.LocationsMustProvideRequiredProperties will catch this, so don't
                // report it here.
                return;
            }

            string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);
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
            Uri uri = location?.PhysicalLocation?.ArtifactLocation?.Uri;
            if (uri == null)
            {
                // Rule SARIF2017.LocationsMustProvideRequiredProperties will catch this, so don't
                // report it here.
                return;
            }

            if (uri.IsAbsoluteUri && uri.Scheme != "file")
            {
                string uriPointer = locationPointer
                    .AtProperty(SarifPropertyName.PhysicalLocation)
                    .AtProperty(SarifPropertyName.ArtifactLocation)
                    .AtProperty(SarifPropertyName.Uri);

                // {0}: '{1}' is not a file path. The GitHub Developer Security Portal only
                // displays results whose locations are specified by file paths, either as
                // relative URIs or as absolute URIs that use the 'file' scheme.
                LogResult(
                    uriPointer,
                    nameof(RuleResources.SARIF2021_LocationsMustBeRelativeUrisOrFilePaths_Error_Default_Text),
                    uri.OriginalString);
            }
        }
    }
}
