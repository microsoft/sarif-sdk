// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseProvideRequiredPhysicalLocationProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.Base1017_ProvideRequiredPhysicalLocationProperties_Error_MissingArtifactLocation_Text),
            nameof(RuleResources.Base1017_ProvideRequiredPhysicalLocationProperties_Error_MissingRegion_Text),
            nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_EndColumnMustNotPrecedeStartColumn_Text),
            nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_EndLineMustNotPrecedeStartLine_Text),
            nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_RegionStartPropertyMustBePresent_Text)
        };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>();

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            if (physicalLocation.Region == null)
            {
                // {0}: The 'physicalLocation' object does not provide a 'region' object. This property is required by the {1} service.
                LogResult(
                    physicalLocationPointer,
                    nameof(RuleResources.Base1017_ProvideRequiredPhysicalLocationProperties_Error_MissingRegion_Text));
            }
            else
            {
                string regionPointer = physicalLocationPointer.AtProperty(SarifPropertyName.Region);
                var jsonPointer = new JsonPointer(regionPointer);
                JToken regionToken = jsonPointer.Evaluate(Context.InputLogToken);
                Region region = physicalLocation.Region;

                if (!region.IsBinaryRegion &&
                    !region.IsLineColumnBasedTextRegion &&
                    !region.IsOffsetBasedTextRegion)
                {
                    // {0}: This 'region' object does not specify 'startLine', 'charOffset', or 'byteOffset'.
                    // As a result, it is impossible to determine whether this 'region' object describes
                    // a line/column text region, a character offset/length text region, or a binary region.
                    LogResult(
                        regionPointer,
                        nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_RegionStartPropertyMustBePresent_Text));
                }

                if (regionToken.HasProperty(SarifPropertyName.EndLine) &&
                    region.EndLine < region.StartLine)
                {
                    string endLinePointer = regionPointer.AtProperty(SarifPropertyName.EndLine);

                    // {0}: In this 'region' object, the 'endLine' property '{1}' is less than the 'startLine'
                    // property '{2}'. The properties of a 'region' object must be internally consistent.
                    LogResult(
                        endLinePointer,
                        nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_EndLineMustNotPrecedeStartLine_Text),
                        region.EndLine.ToInvariantString(),
                        region.StartLine.ToInvariantString());
                }

                if (RegionIsOnOneLine(region, regionToken) &&
                    regionToken.HasProperty(SarifPropertyName.EndColumn) &&
                    region.EndColumn < region.StartColumn)
                {
                    string endColumnPointer = regionPointer.AtProperty(SarifPropertyName.EndColumn);

                    // {0}: In this 'region' object, the 'endColumn' property '{1}' is less than the 'startColumn'
                    // property '{2}'. The properties of a 'region' object must be internally consistent.
                    LogResult(
                        endColumnPointer,
                        nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_EndColumnMustNotPrecedeStartColumn_Text),
                        region.EndColumn.ToInvariantString(),
                        region.StartColumn.ToInvariantString());
                }
            }

            if (physicalLocation.ArtifactLocation == null)
            {
                // {0}: This 'physicalLocation' object does not provide an 'artifactLocation' object. This property is required by the {1} service.
                LogResult(
                    physicalLocationPointer,
                    nameof(RuleResources.Base1017_ProvideRequiredPhysicalLocationProperties_Error_MissingArtifactLocation_Text));
            }
        }

        private static bool RegionIsOnOneLine(Region region, JToken regionToken)
        {
            return regionToken.HasProperty(SarifPropertyName.EndLine)
                ? region.StartLine == region.EndLine
                : true;
        }
    }
}
