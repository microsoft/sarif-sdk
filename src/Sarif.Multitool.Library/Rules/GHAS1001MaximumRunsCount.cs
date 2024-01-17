// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GHAS1001MaximumRunsCount : Base1001MaximumRunsCount
    {
        /// <summary>
        /// GHAS1001
        /// </summary>
        public override string Id => RuleId.GHASMaximumRunsCount;

        protected override string ServiceName => RuleResources.ServiceName_GHAS;

        public override int MaximumRuns => 20;

        protected override void Analyze(SarifLog sarifLog, string runPointer)
        {
            base.Analyze(sarifLog, runPointer);
        }
    }
}
