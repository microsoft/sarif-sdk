// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

using Newtonsoft.Json.Linq;

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
            // This approach may cause occasional false negatives.
            ArtifactLocation firstLocationInArtifactsArray = run?.Artifacts?.FirstOrDefault()?.Location;
            ArtifactLocation firstlocationInResults = run?.Results?.FirstOrDefault()?.Locations?.FirstOrDefault()?.PhysicalLocation?.ArtifactLocation;

            if (firstLocationInArtifactsArray == null || firstlocationInResults == null)
            {
                return;
            }

            string firstArtifactPointer = runPointer
                .AtProperty(SarifPropertyName.Artifacts).AtIndex(0);

            string firstResultLocationPointer = runPointer
                .AtProperty(SarifPropertyName.Results).AtIndex(0)
                .AtProperty(SarifPropertyName.Locations).AtIndex(0)
                .AtProperty(SarifPropertyName.PhysicalLocation)
                .AtProperty(SarifPropertyName.ArtifactLocation);

            if (HasResultLocationsWithUriAndIndex(firstResultLocationPointer) && HasLocationOnlyArtifacts(firstArtifactPointer))
            {
                // {0}: Placeholder_SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text
                LogResult(
                    firstArtifactPointer,
                    nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text));
            }
        }

        private bool HasResultLocationsWithUriAndIndex(string resultPointer)
        {
            JToken resultToken = resultPointer.ToJToken(Context.InputLogToken);
            return
                resultToken.HasProperty(SarifPropertyName.Uri) &&
                resultToken.HasProperty(SarifPropertyName.Index);
        }

        private bool HasLocationOnlyArtifacts(string artifactPointer)
        {
            JToken artifactToken = artifactPointer.ToJToken(Context.InputLogToken);
            return
                artifactToken.HasProperty(SarifPropertyName.Location) &&
                artifactToken.Children().Count() == 1;
        }
    }
}
