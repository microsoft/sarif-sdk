// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GhasProvideRequiredResultPropertties
        : BaseProvideRequiredResultProperties
    {
        /// <summary>
        /// GHAS1015
        /// </summary>
        public override string Id => RuleId.GHASProvideRequiredResultProperties;

        protected override string ServiceName => RuleResources.ServiceName_GHAS;

        public AdoProvideRequiredRunPropertties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            base.Analyze(result, resultPointer);
        }
    }
}
