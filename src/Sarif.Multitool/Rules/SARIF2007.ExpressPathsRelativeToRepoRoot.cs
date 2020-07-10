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
        /// Provide information that makes it possible to determine the repo-relative locations of
        /// files that contain analysis results.
        ///
        /// Each element of the 'versionControlProvenance' array is a 'versionControlDetails' object
        /// that describes a repository containing files that were analyzed. 'versionControlDetails.mappedTo'
        /// defines the file system location to which the root of that repository is mapped. If
        /// 'mappedTo.uriBaseId' is present, and if result locations are expressed relative to that
        /// 'uriBaseId', then the repo-relative location of each result can be determined.
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
                        // {0}: The 'versionControlDetails' object that describes the repository '{1}'
                        // does not provide 'mappedTo.uriBaseId'. As a result, it will not be possible
                        // to determine the repo-relative location of files containing analysis results
                        // for this repository.
                        LogResult(
                            versionControlDetailsPointer,
                            nameof(RuleResources.SARIF2007_ExpressPathsRelativeToRepoRoot_Warning_ProvideUriBaseIdForMappedTo_Text),
                            run.VersionControlProvenance[i].RepositoryUri.OriginalString);
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
            if (this.uriBaseIds.Any() && location.PhysicalLocation != null)
            {
                string physicalLocation = locationPointer.AtProperty(SarifPropertyName.PhysicalLocation);
                if (location.PhysicalLocation.ArtifactLocation != null)
                {
                    string artifactLocation = physicalLocation.AtProperty(SarifPropertyName.ArtifactLocation);
                    if (string.IsNullOrWhiteSpace(location.PhysicalLocation.ArtifactLocation.UriBaseId)
                        || !this.uriBaseIds.Contains(location.PhysicalLocation.ArtifactLocation.UriBaseId))
                    {
                        // {0}: This result location does not provide any of the 'uriBaseId' values
                        // that specify repository locations: {1}. As a result, it will not be possible
                        // to determine the location of the file containing this result relative to the
                        // root of the repository that contains it.
                        LogResult(
                            artifactLocation,
                            nameof(RuleResources.SARIF2007_ExpressPathsRelativeToRepoRoot_Warning_ExpressResultLocationsRelativeToMappedTo_Text),
                            string.Join(", ", this.uriBaseIds));
                    }
                }
            }
        }
    }
}
