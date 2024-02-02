// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GhasProvideRequiredRunPropertties
        : BaseProvideRequiredRunProperties
    {
        /// <summary>
        /// GHAS1014
        /// </summary>
        public override string Id => RuleId.GHASProvideRequiredRunProperties;

        protected override string ServiceName => RuleResources.ServiceName_ADO;

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
