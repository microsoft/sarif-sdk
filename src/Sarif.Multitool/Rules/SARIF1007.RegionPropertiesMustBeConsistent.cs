// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RegionPropertiesMustBeConsistent : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF1007
        /// </summary>
        public override string Id => RuleId.RegionPropertiesMustBeConsistent;

        /// <summary>
        /// The properties of a 'region' object must be consistent.
        ///
        /// SARIF can specify a 'region' (a contiguous portion of a file) in a variety of ways:
        /// with line and column numbers, with a character offset and count, or with a byte offset
        /// and count.The specification states certain constraints on these properties, both within
        /// each property group (for example, the start line cannot be greater than end line) and
        /// between the groups(for example, if more than one group is present, they must independently
        /// specify the same portion of the file). See the SARIF specification
        /// ([3.30] (https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317685)).
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_EndLineMustNotPrecedeStartLine_Text),
            nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_EndColumnMustNotPrecedeStartColumn_Text),
            nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_RegionStartPropertyMustBePresent_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        protected override void Analyze(Region region, string regionPointer)
        {
            var jsonPointer = new JsonPointer(regionPointer);
            JToken regionToken = jsonPointer.Evaluate(Context.InputLogToken);

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

        private static bool RegionIsOnOneLine(Region region, JToken regionToken)
        {
            return regionToken.HasProperty(SarifPropertyName.EndLine)
                ? region.StartLine == region.EndLine
                : true;
        }
    }
}
