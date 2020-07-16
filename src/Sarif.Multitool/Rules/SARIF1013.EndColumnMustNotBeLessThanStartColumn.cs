// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndColumnMustNotBeLessThanStartColumn : SarifValidationSkimmerBase
    {
        public EndColumnMustNotBeLessThanStartColumn() : base(
            RuleId.EndColumnMustNotBeLessThanStartColumn,
            RuleResources.SARIF1013_EndColumnMustNotBeLessThanStartColumn,
            FailureLevel.Error,
            new string[] { nameof(RuleResources.SARIF1013_Default) }
            )
        { }

        protected override void Analyze(Region region, string regionPointer)
        {
            var jsonPointer = new JsonPointer(regionPointer);
            JToken regionToken = jsonPointer.Evaluate(Context.InputLogToken);

            if (RegionIsOnOneLine(region, regionToken) &&
                regionToken.HasProperty(SarifPropertyName.EndColumn) &&
                region.EndColumn < region.StartColumn)
            {
                string endColumnPointer = regionPointer.AtProperty(SarifPropertyName.EndColumn);

                LogResult(
                    endColumnPointer,
                    nameof(RuleResources.SARIF1013_Default),
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
