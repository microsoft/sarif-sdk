// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public class EndLineMustNotBeLessThanStartLine : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF012_EndLineMustNotBeLessThanStartLine;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF012
        /// </summary>
        public override string Id => RuleId.EndLineMustNotBeLessThanStartLine;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF012_Default)
                };
            }
        }

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
                    nameof(RuleResources.SARIF012_Default),
                    region.EndLine.ToInvariantString(),
                    region.StartLine.ToInvariantString());
            }
        }
    }
}
