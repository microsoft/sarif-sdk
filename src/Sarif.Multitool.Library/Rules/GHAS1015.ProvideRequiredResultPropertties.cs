// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GhasProvideRequiredResultPropertties
        : BaseProvideRequiredResultProperties
    {
        /// <summary>
        /// GHAS1015
        /// </summary>
        public override string Id => RuleId.GHASProvideRequiredResultProperties;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>([RuleKind.Ghas]);

        protected override string ServiceName => RuleResources.ServiceName_GHAS;

        public GhasProvideRequiredResultPropertties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            base.Analyze(result, resultPointer);
        }
    }
}
