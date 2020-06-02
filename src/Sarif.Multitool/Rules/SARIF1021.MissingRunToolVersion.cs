// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class MissingRunToolVersion : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            //Text = RuleResources.SARIF1021_MissingRunToolVersion
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1021
        /// </summary>
        public override string Id => RuleId.MissingRunToolVersion;

        protected override void Analyze(Run run, string runPointer)
        {
            //
        }
}
}
