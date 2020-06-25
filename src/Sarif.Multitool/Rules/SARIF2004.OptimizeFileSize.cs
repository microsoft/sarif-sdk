// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class OptimizeFileSize : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2004
        /// </summary>
        public override string Id => RuleId.OptimizeFileSize;

        /// <summary>
        /// Placeholder_SARIF2004_OptimizeFileSize_FullDescription_Text
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2004_OptimizeFileSize_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
                    nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text)
                };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        protected override void Analyze(Run run, string runPointer)
        {
            // We only verify first item in the results and artifacts array,
            // since tools will typically generate similar nodes.
            // This approach may cause false negatives.
            ArtifactLocation firstLocationInArtifactsArray = run.Artifacts[0].Location;
            ArtifactLocation firstlocationInResults = run.Results[0].Locations[0].PhysicalLocation.ArtifactLocation;

            if (!ArrayBasedLocationHasAdditionalInfo(firstLocationInArtifactsArray, firstlocationInResults))
            {
                // {0}: Placeholder_SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text
                LogResult(
                    runPointer.AtProperty(SarifPropertyName.Artifacts),
                    nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text));
            }
        }

        private bool ArrayBasedLocationHasAdditionalInfo(ArtifactLocation arrayBasedLocation, ArtifactLocation resultBasedLocation)
        {
            if (resultBasedLocation.Uri == null && arrayBasedLocation.Uri != null)
            {
                return true;
            }

            if (resultBasedLocation.UriBaseId == null && arrayBasedLocation.UriBaseId != null)
            {
                return true;
            }

            if (resultBasedLocation.Description == null && arrayBasedLocation.Description != null)
            {
                return true;
            }

            return false;
        }
    }
}
