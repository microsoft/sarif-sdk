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

        // The GitHub Security Portal only displays results whose locations are specified by file
        // paths, either as relative URIs or as absolute URIs that use the 'file' scheme.
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

            for (int i = 0; i < result.Locations.Count; i++)
            {
                Location location = result.Locations[i];

                Uri uri = location?.PhysicalLocation?.ArtifactLocation?.Uri;
                if (uri == null)
                {
                    // Rule SARIF2017.LocationsMustProvideRequiredProperties will catch this, so don't
                    // report it here.
                    continue;
                }

                if (uri.IsAbsoluteUri && uri.Scheme != "file")
                {
                    string uriPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.ArtifactLocation)
                        .AtProperty(SarifPropertyName.Uri);

                    // {0}: An absolute URI with the '{1}' scheme does not specify a file path.
                    // The GitHub Security Portal only displays results whose locations are
                    // specified by file paths, either as relative URIs or as absolute URIs that
                    // use the 'file' scheme.
                    LogResult(
                        uriPointer,
                        nameof(RuleResources.SARIF2021_LocationsMustBeRelativeUrisOrFilePaths_Error_Default_Text),
                        uri.Scheme);
                }
            }
        }
    }
}
