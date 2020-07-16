// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndLineMustNotBeLessThanStartLine : SarifValidationSkimmerBase
    {
        public EndLineMustNotBeLessThanStartLine() : base(
            RuleId.EndLineMustNotBeLessThanStartLine,
            RuleResources.SARIF1012_EndLineMustNotBeLessThanStartLine,
            FailureLevel.Error,
            new string[] { nameof(RuleResources.SARIF1012_Default) }
            )
        { }

        protected override void Analyze(Region region, string regionPointer)
        {
            var jsonPointer = new JsonPointer(regionPointer);
            Newtonsoft.Json.Linq.JToken regionToken = jsonPointer.Evaluate(Context.InputLogToken);

            if (regionToken.HasProperty(SarifPropertyName.EndLine) &&
                region.EndLine < region.StartLine)
            {
                string endLinePointer = regionPointer.AtProperty(SarifPropertyName.EndLine);

                LogResult(
                    endLinePointer,
                    nameof(RuleResources.SARIF1012_Default),
                    region.EndLine.ToInvariantString(),
                    region.StartLine.ToInvariantString());
            }
        }
    }
}
