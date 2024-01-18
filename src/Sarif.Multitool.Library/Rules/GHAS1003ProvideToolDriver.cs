// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GhasProvideToolDriver
        : BaseProvideToolDriver
    {
        /// <summary>
        /// GHAS1003
        /// </summary>
        public override string Id => RuleId.GHASProvideToolDriverProperties;

        public override RuleKinds Kinds => RuleKinds.Ado;

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
