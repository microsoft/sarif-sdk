// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ExpressUriBaseIdsCorrectly : SarifValidationSkimmerBase
    {
        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override string Id => RuleId.ExpressUriBaseIdsCorrectly;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1004_Default)
        };

        protected override void Analyze(ArtifactLocation fileLocation, string fileLocationPointer)
        {
            if (fileLocation.UriBaseId != null && fileLocation.Uri.IsAbsoluteUri)
            {
                LogResult(
                    fileLocationPointer.AtProperty(SarifPropertyName.Uri),
                    nameof(RuleResources.SARIF1004_Default),
                    fileLocation.Uri.OriginalString);
            }
        }
    }
}
