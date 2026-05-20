// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class SARIFProvideRequiredRegionProperties : SarifValidationSkimmerBase
    {
        public SARIFProvideRequiredRegionProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// SARIF2017
        /// </summary>
        public override string Id => RuleId.SARIFProvideRequiredRegionProperties;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Sarif });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF2017_ProvideRequiredRegionProperties_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.SARIF2017_ProvideRequiredRegionProperties_Warning_MissingRegion_Text),
            nameof(RuleResources.SARIF2017_ProvideRequiredRegionProperties_Warning_MissingRegionProperty_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!(result.Locations?.Any() == true))
            {
                return;
            }

            for (int i = 0; i < result.Locations.Count; i++)
            {
                Location location = result.Locations[i];
                PhysicalLocation physicalLocation = location.PhysicalLocation;
                if (physicalLocation == null)
                {
                    continue;
                }

                if (physicalLocation.Region == null)
                {
                    string physicalLocationPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation);

                    // {0}: The 'region' property is absent. Results should provide a 'region'
                    // object with line and column information so that consumers can display
                    // the precise location.
                    LogResult(
                        physicalLocationPointer,
                        nameof(RuleResources.SARIF2017_ProvideRequiredRegionProperties_Warning_MissingRegion_Text));
                }
                else if (!physicalLocation.Region.IsLineColumnBasedTextRegion
                      && !physicalLocation.Region.IsOffsetBasedTextRegion
                      && !physicalLocation.Region.IsBinaryRegion)
                {
                    // Region is present but carries no positional information at all
                    // (no startLine, no charOffset, no byteOffset). Per SARIF \u00a73.30 a
                    // region is identified by *one of* line/column, char-offset, or
                    // byte-offset; an entirely-empty region object is meaningless.
                    string regionPointer = resultPointer
                        .AtProperty(SarifPropertyName.Locations)
                        .AtIndex(i)
                        .AtProperty(SarifPropertyName.PhysicalLocation)
                        .AtProperty(SarifPropertyName.Region);

                    // {0}: The 'region' object carries no positional information. Express the
                    // region in one of the three SARIF \u00a73.30 forms: line/column (startLine,
                    // optional startColumn/endLine/endColumn); character offset (charOffset,
                    // optional charLength); or byte offset (byteOffset, optional byteLength).
                    // For a finding that applies to the whole artifact, prefer a binary region
                    // (Region.ForEntireArtifact).
                    LogResult(
                        regionPointer,
                        nameof(RuleResources.SARIF2017_ProvideRequiredRegionProperties_Warning_MissingRegionProperty_Text));
                }
            }
        }
    }
}
