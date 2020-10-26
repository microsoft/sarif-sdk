﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    ///  GitHub Advanced Security code scanning only displays results whose locations are specified
    ///  by file paths, either as relative URIs or as absolute URIs that use the 'file' scheme.
    /// </summary>
    public class LocationsMustBeRelativeUrisOrFilePaths : SarifValidationSkimmerBase
    {
        public LocationsMustBeRelativeUrisOrFilePaths() : base(
            RuleId.LocationsMustBeRelativeUrisOrFilePaths,
            RuleResources.GH1005_LocationsMustBeRelativeUrisOrFilePaths_FullDescription_Text,
            FailureLevel.Error,
            new string[] { nameof(RuleResources.GH1005_LocationsMustBeRelativeUrisOrFilePaths_Error_Default_Text) },
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
                // Rule GH1001.ProvideRequiredLocationProperties will catch this, so don't
                // report it here.
                return;
            }

            if (uri.IsAbsoluteUri && uri.Scheme != "file")
            {
                string uriPointer = locationPointer
                    .AtProperty(SarifPropertyName.PhysicalLocation)
                    .AtProperty(SarifPropertyName.ArtifactLocation)
                    .AtProperty(SarifPropertyName.Uri);

                // {0}: '{1}' is not a file path. GitHub Advanced Security code scanning only
                // displays results whose locations are specified by file paths, either as
                // relative URIs or as absolute URIs that use the 'file' scheme.
                LogResult(
                    uriPointer,
                    nameof(RuleResources.GH1005_LocationsMustBeRelativeUrisOrFilePaths_Error_Default_Text),
                    uri.OriginalString);
            }
        }
    }
}
