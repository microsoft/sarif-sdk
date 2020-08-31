// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideRequiredRelatedLocationProperties : SarifValidationSkimmerBase
    {
        /// <summary>
        /// DSP1007
        /// </summary>
        public override string Id => RuleId.ProvideRequiredRelatedLocationProperties;

        // The GitHub Developer Security Portal (DSP) will reject a SARIF file that includes a
        // "related location" with no 'message' property. This is a bug in the DSP. You can set
        // 'message' to an empty string if you don't have anything else to say about the location.
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.DSP1007_ProvideRequiredRelatedLocationProperties_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.DSP1007_ProvideRequiredRelatedLocationProperties_Error_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override bool EnabledByDefault => false;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.RelatedLocations?.Any() == true)
            {
                string relatedLocationsPointer = resultPointer.AtProperty(SarifPropertyName.RelatedLocations);
                for (int i = 0; i < result.RelatedLocations.Count; i++)
                {
                    ValidateRelatedLocation(result.RelatedLocations[i], relatedLocationsPointer.AtIndex(i));
                }
            }
        }

        private void ValidateRelatedLocation(Location relatedLocation, string relatedLocationPointer)
        {
            if (relatedLocation.Message == null)
            {
                // {0}: This related location does not have a 'message' property, so the the GitHub
                // Developer Security Portal (DSP) will reject the entire log file. This is a bug
                // in the DSP. You can set 'message' to an empty string if you don't have anything
                // else to say about the location.
                LogResult(
                    relatedLocationPointer,
                    nameof(RuleResources.DSP1007_ProvideRequiredRelatedLocationProperties_Error_Default_Text));
            }
        }
    }
}
