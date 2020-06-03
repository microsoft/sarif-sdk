// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class MissingToolVersion : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            Text = RuleResources.SARIF1021_MissingToolVersion
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1021
        /// </summary>
        public override string Id => RuleId.MissingToolVersion;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1021_Default)
        };

        protected override void Analyze(Tool tool, string pointer)
        {
            // 'Driver' is a required property, hence we do not need a null check for it. 
            if (string.IsNullOrWhiteSpace(tool.Driver.Version))
            {
                LogResult(pointer, nameof(RuleResources.SARIF1021_MissingToolVersion));
                return;
            }
        }
    }
}
