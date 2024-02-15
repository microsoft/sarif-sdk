// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AdoProvideRequiredSarifLogProperties
        : BaseProvideRequiredSarifLogProperties
    {
        /// <summary>
        /// ADO1013
        /// </summary>
        public override string Id => RuleId.ADOProvideRequiredSarifLogProperties;

        public override MultiformatMessageString FullDescription => new() { Text = RuleResources.ADO1013_ProvideRequiredSarifLogProperties_FullDescription_Text };

        public override HashSet<RuleKind> RuleKinds => new([RuleKind.Ado]);

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        public override int MaximumRuns => 20;

        public AdoProvideRequiredSarifLogProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(SarifLog sarifLog, string runPointer)
        {
            base.Analyze(sarifLog, runPointer);
        }
    }
}
