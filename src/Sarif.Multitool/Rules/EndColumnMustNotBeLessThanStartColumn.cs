// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndColumnMustNotBeLessThanStartColumn : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            Text = RuleResources.SARIF1013_EndColumnMustNotBeLessThanStartColumn
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1013
        /// </summary>
        public override string Id => RuleId.EndColumnMustNotBeLessThanStartColumn;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1013_Default)
        };

        protected override void Analyze(Region region, string regionPointer)
        {
            var jsonPointer = new JsonPointer(regionPointer);
            var regionToken = jsonPointer.Evaluate(Context.InputLogToken);

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
