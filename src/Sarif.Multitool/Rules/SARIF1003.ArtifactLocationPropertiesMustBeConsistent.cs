// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ArtifactLocationPropertiesMustBeConsistent : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF1003
        /// </summary>
        public override string Id => RuleId.ArtifactLocationPropertiesMustBeConsistent;

        /// <summary>
        /// Placeholder_SARIF1003_ArtifactLocationPropertiesMustBeConsistent_FullDescription_Text
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1003_ArtifactLocationPropertiesMustBeConsistent_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1003_ArtifactLocationPropertiesMustBeConsistent_Error_UriBaseIdRequiresRelativeUri_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        protected override void Analyze(ArtifactLocation fileLocation, string fileLocationPointer)
        {
            // UriBaseIdRequiresRelativeUri: The 'uri' property of 'fileLocation' must be a relative uri, since 'uriBaseId' is present.
            if (fileLocation.UriBaseId != null && fileLocation.Uri.IsAbsoluteUri)
            {
                // {0}: {1} Placeholder_SARIF1003_ArtifactLocationPropertiesMustBeConsistent_Error_UriBaseIdRequiresRelativeUri_Text
                LogResult(
                    fileLocationPointer.AtProperty(SarifPropertyName.Uri),
                    nameof(RuleResources.SARIF1003_ArtifactLocationPropertiesMustBeConsistent_Error_UriBaseIdRequiresRelativeUri_Text),
                    fileLocation.Uri.OriginalString);
            }
        }
    }
}
