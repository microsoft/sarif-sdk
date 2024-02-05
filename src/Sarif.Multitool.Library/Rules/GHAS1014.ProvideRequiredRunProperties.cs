// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GhasProvideRequiredRunPropertties
        : BaseProvideRequiredRunProperties
    {
        /// <summary>
        /// GHAS1014
        /// </summary>
        public override string Id => RuleId.GHASProvideRequiredRunProperties;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>([RuleKind.Ghas]);

        protected override string ServiceName => RuleResources.ServiceName_GHAS;

        public GhasProvideRequiredRunPropertties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Run run, string runPointer)
        {
            // run.results is chcked by the base class.
            base.Analyze(run, runPointer);
        }
    }
}
