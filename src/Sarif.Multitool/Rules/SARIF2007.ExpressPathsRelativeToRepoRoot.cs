// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ExpressPathsRelativeToRepoRoot : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2007
        /// </summary>
        public override string Id => RuleId.ExpressPathsRelativeToRepoRoot;
        
        /// <summary>
        /// Placeholder
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2007_ExpressPathsRelativeToRepoRoot_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2007_ExpressPathsRelativeToRepoRoot_Warning_ExpressResultLocationsRelativeToMappedTo_Text),
            nameof(RuleResources.SARIF2007_ExpressPathsRelativeToRepoRoot_Warning_ProvideUriBaseIdForMappedTo_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        private HashSet<string> uriBaseIds;

        protected override void Analyze(Run run, string runPointer)
        {
            this.uriBaseIds = new HashSet<string>();

            if (run.VersionControlProvenance != null)
            {
                string versionControlProvenancePointer = runPointer.AtProperty(SarifPropertyName.VersionControlProvenance);

                for (int i = 0; i < run.VersionControlProvenance.Count; i++)
                {
                    string versionControlDetailsPointer = versionControlProvenancePointer.AtIndex(i);
                    if (run.VersionControlProvenance[i].MappedTo == null || string.IsNullOrWhiteSpace(run.VersionControlProvenance[i].MappedTo.UriBaseId))
                    {
                        // {0}: Placeholder
                        LogResult(
                            versionControlDetailsPointer,
                            nameof(RuleResources.SARIF2007_ExpressPathsRelativeToRepoRoot_Warning_ProvideUriBaseIdForMappedTo_Text));
                    }
                    else
                    {
                        this.uriBaseIds.Add(run.VersionControlProvenance[i].MappedTo.UriBaseId);
                    }
                }
            }
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.Locations != null && this.uriBaseIds.Any())
            {
                string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);
                for (int i = 0; i < result.Locations.Count; i++)
                {
                    AnalyzeLocation(result.Locations[i], locationsPointer.AtIndex(i));
                }
            }
        }

        private void AnalyzeLocation(Location location, string locationPointer)
        {
            if (location.PhysicalLocation != null)
            {
                string physicalLocation = locationPointer.AtProperty(SarifPropertyName.PhysicalLocation);
                if (location.PhysicalLocation.ArtifactLocation != null)
                {
                    string artifactLocation = physicalLocation.AtProperty(SarifPropertyName.ArtifactLocation);
                    if (string.IsNullOrWhiteSpace(location.PhysicalLocation.ArtifactLocation.UriBaseId)
                        || !this.uriBaseIds.Contains(location.PhysicalLocation.ArtifactLocation.UriBaseId))
                    {
                        // {0}: Placeholder
                        LogResult(
                            artifactLocation,
                            nameof(RuleResources.SARIF2007_ExpressPathsRelativeToRepoRoot_Warning_ExpressResultLocationsRelativeToMappedTo_Text));
                    }
                }
            }
        }
    }
}
