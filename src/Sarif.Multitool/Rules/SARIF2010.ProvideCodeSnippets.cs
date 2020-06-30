// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        /// Placeholder_SARIF2010_ProvideCodeSnippets_FullDescription_Text
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2010_ProvideCodeSnippets_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2010_ProvideCodeSnippets_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

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
            Region region = location.PhysicalLocation?.Region;
            Region contextRegion = location.PhysicalLocation?.ContextRegion;

            if (region != null && contextRegion != null)
            {
                AnalyzeRegion(
                    region,
                    locationPointer
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.Region));

                AnalyzeRegion(
                    contextRegion,
                    locationPointer
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.ContextRegion));
            }
        }

        private void AnalyzeRegion(Region region, string regionPointer)
        {

            if (region.Snippet == null)
            {
                // Placeholder_SARIF2010_ProvideCodeSnippets_Note_Default_Text
                LogResult(
                    regionPointer,
                    nameof(RuleResources.SARIF2010_ProvideCodeSnippets_Note_Default_Text));
            }
        }
    }
}
