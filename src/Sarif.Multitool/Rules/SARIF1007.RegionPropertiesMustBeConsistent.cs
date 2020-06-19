// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RegionPropertiesMustBeConsistent : SarifValidationSkimmerBase
    {
        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_FullDescription_Text
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override string Id => RuleId.RegionPropertiesMustBeConsistent;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_EndLineMustNotPrecedeStartLine_Text),
            nameof(RuleResources.SARIF1007_RegionPropertiesMustBeConsistent_Error_EndColumnMustNotPrecedeStartColumn_Text)
        };

        protected override void Analyze(Region region, string regionPointer)
        {
            var jsonPointer = new JsonPointer(regionPointer);
            JToken regionToken = jsonPointer.Evaluate(Context.InputLogToken);

            if (regionToken.HasProperty(SarifPropertyName.EndLine) &&
                region.EndLine < region.StartLine)
            {
                string endLinePointer = regionPointer.AtProperty(SarifPropertyName.EndLine);

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
