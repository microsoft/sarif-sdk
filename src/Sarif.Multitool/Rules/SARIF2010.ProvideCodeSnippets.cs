// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideCodeSnippets : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2010
        /// </summary>
        public override string Id => RuleId.ProvideCodeSnippets;

        /// <summary>
        /// Provide code snippets to enable users to see the code that triggered each result,
        /// even if they are not enlisted in the code.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2010_ProvideCodeSnippets_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2010_ProvideCodeSnippets_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        private IList<Artifact> artifacts;
        private IDictionary<string, ArtifactLocation> originalUriBaseIds;

        protected override void Analyze(Run run, string runPointer)
        {
            this.artifacts = run.Artifacts;
            this.originalUriBaseIds = run.OriginalUriBaseIds;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.Locations != null)
            {
                string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);
                for (int i = 0; i < result.Locations.Count; i++)
                {
                    AnalyzeResultLocation(result.Locations[i], locationsPointer.AtIndex(i));
                }
            }
        }

        private void AnalyzeResultLocation(Location location, string locationPointer)
        {
            if (AnalyzeArtifactLocation(location.PhysicalLocation?.ArtifactLocation))
            {
                AnalyzeRegion(
                    location.PhysicalLocation?.Region,
                    locationPointer
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.Region));

                AnalyzeRegion(
                    location.PhysicalLocation?.ContextRegion,
                    locationPointer
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.ContextRegion));
            }
        }

        private void AnalyzeRegion(Region region, string regionPointer)
        {
            if (region != null && region.Snippet == null)
            {
                // {0}: The 'region' object in this result location does not provide a 'snippet'
                // property. Providing a code snippet enables users to see the source code that
                // triggered the result, even if they are not enlisted in the code.
                LogResult(
                    regionPointer,
                    nameof(RuleResources.SARIF2010_ProvideCodeSnippets_Note_Default_Text));
            }
        }

        private bool AnalyzeArtifactLocation(ArtifactLocation artifactLocation)
        {
            // no artifactLocation / no artifacts, so we should look for the snippet
            if (artifactLocation == null || this.artifacts == null)
            {
                return true;
            }

            artifactLocation.TryReconstructAbsoluteUri(this.originalUriBaseIds, out Uri resolvedUri);

            foreach (Artifact artifact in this.artifacts)
            {
                // content/text doesn't exist, continue to next
                if (string.IsNullOrEmpty(artifact.Contents?.Text))
                {
                    continue;
                }

                artifact.Location.TryReconstructAbsoluteUri(this.originalUriBaseIds, out Uri artifactUri);

                if (resolvedUri != null && artifactUri != null)
                {
                    if (artifactUri.AbsolutePath.Equals(resolvedUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                else
                {
                    // couldn't generate the absoluteUri's, so let's compare everything
                    if (this.artifacts.Any(a => a.Location?.Uri.OriginalString == artifactLocation.Uri.OriginalString
                        && a.Location?.UriBaseId == artifactLocation.UriBaseId))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
