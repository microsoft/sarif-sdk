// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class FileUrisShouldBeRelative : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2016
        /// </summary>
        public override string Id => RuleId.FileUrisShouldBeRelative;

        /// <summary>
        /// When an artifact location refers to a file on the local file system, specify a relative reference
        /// for the uri property and provide a uriBaseId property, rather than specifying an absolute URI.
        /// 
        /// There are several advantages to this approach:
        /// Portability: A log file that contains relative references together with uriBaseI properties can
        /// be interpreted on a machine where the files are located at a different absolute location.
        /// 
        /// Determinism: A log file that uses uriBaseId properties has a better chance of being “deterministic”;
        /// that is, of being identical from run to run if none of its inputs have changed, even if those runs
        /// occur on machines where the files are located at different absolute locations.
        /// 
        /// Security: The use of uriBaseId properties avoids the persistence of absolute path names in the
        /// log file.Absolute path names can reveal information that might be sensitive.
        /// 
        /// Semantics: Assuming the reader of the log file (an end user or another tool) has the necessary
        /// context, they can understand the meaning of the location specified by the uri property, for
        /// example, “this is a source file”.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2016_FileUrisShouldBeRelative_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2016_FileUrisShouldBeRelative_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Results != null)
            {
                AnalyzeResults(run.Results, runPointer.AtProperty(SarifPropertyName.Results));
            }
        }

        private void AnalyzeResults(IList<Result> results, string resultsPointer)
        {
            for (int i = 0; i < results.Count; i++)
            {
                string resultPointer = resultsPointer.AtIndex(i);
                Result result = results[i];

                if (result.Locations != null)
                {
                    AnalyzeResultLocations(result.Locations, resultPointer.AtProperty(SarifPropertyName.Locations));
                }
            }
        }

        private void AnalyzeResultLocations(IList<Location> locations, string locationsPointer)
        {
            for (int i = 0; i < locations.Count; i++)
            {
                string locationPointer = locationsPointer.AtIndex(i);
                Location location = locations[i];

                if (location.PhysicalLocation?.ArtifactLocation.Uri != null)
                {
                    if (location.PhysicalLocation.ArtifactLocation.Uri.IsAbsoluteUri && location.PhysicalLocation.ArtifactLocation.Uri.IsFile)
                    {
                        // {0}: The file location '{1}' is specified with absolute URI. Prefer a relative
                        // reference together with a uriBaseId property.
                        LogResult(
                            locationPointer.AtProperty(SarifPropertyName.PhysicalLocation).AtProperty(SarifPropertyName.ArtifactLocation).AtProperty(SarifPropertyName.Uri),
                            nameof(RuleResources.SARIF2016_FileUrisShouldBeRelative_Note_Default_Text),
                            location.PhysicalLocation.ArtifactLocation.Uri.OriginalString);
                    }
                }
            }
        }
    }
}
