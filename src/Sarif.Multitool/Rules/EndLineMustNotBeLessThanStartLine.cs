// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndLineMustNotBeLessThanStartLine : SarifValidationSkimmerBase
    {
        private readonly Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1012_EndLineMustNotBeLessThanStartLine
        };

        public override Message FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1012
        /// </summary>
        public override string Id => RuleId.EndLineMustNotBeLessThanStartLine;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1012_Default)
        };

        protected override void Analyze(Region region, string regionPointer)
        {
            var jsonPointer = new JsonPointer(regionPointer);
            var regionToken = jsonPointer.Evaluate(Context.InputLogToken);

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
