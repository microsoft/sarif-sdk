// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GhasProvideToolDriver
        : BaseProvideRequiredToolProperties
    {
        /// <summary>
        /// GH1018
        /// </summary>
        public override string Id => RuleId.GHASProvideRequiredToolProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString() { Text = RuleResources.GH1018_ProvideRequiredToolProperties_FullDescription_Text };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Ghas });

        protected override string ServiceName => RuleResources.ServiceName_GHAS;

        public GhasProvideToolDriver()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Run run, string runPointer)
        {
            // run.tool is chcked by the base class.
            base.Analyze(run, runPointer);
        }
    }
}
