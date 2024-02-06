// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AdoProvideRequiredResultPropertties
        : BaseProvideRequiredResultProperties
    {
        /// <summary>
        /// ADO1015
        /// </summary>
        public override string Id => RuleId.ADOProvideRequiredResultProperties;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Ado });

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        public AdoProvideRequiredResultPropertties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            base.Analyze(result, resultPointer);

            if (string.IsNullOrWhiteSpace(result.RuleId))
            {
                // {0}: This 'result' object does not provide a 'ruleId' value. This property is required by the {1} service.
                LogResult(
                    resultPointer,
                    nameof(RuleResources.ADO1015_ProvideRequiredResultProperties_Error_MissingRuleId_Text),
                    this.ServiceName);
            }
        }
    }
}
