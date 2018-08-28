// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UriBaseIdRequiresRelativeUri : SarifValidationSkimmerBase
    {
        private Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1014_UriBaseIdRequiresRelativeUri
        };

        public override Message FullDescription => _fullDescription;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF1014
        /// </summary>
        public override string Id => RuleId.UriBaseIdRequiresRelativeUri;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1014_Default)
        };

        protected override void Analyze(FileLocation fileLocation, string fileLocationPointer)
        {
            if (fileLocation.UriBaseId != null && fileLocation.Uri.IsAbsoluteUri)
            {
                LogResult(
                    fileLocationPointer.AtProperty(SarifPropertyName.Uri),
                    nameof(RuleResources.SARIF1014_Default),
                    fileLocation.Uri.OriginalString);
            }
        }
    }
}
