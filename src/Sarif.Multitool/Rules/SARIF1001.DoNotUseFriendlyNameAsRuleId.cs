// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DoNotUseFriendlyNameAsRuleId : SarifValidationSkimmerBase
    {
        public DoNotUseFriendlyNameAsRuleId() : base(
            RuleId.DoNotUseFriendlyNameAsRuleId,
            RuleResources.SARIF1001_DoNotUseFriendlyNameAsRuleIdDescription,
            FailureLevel.Warning,
            new string[] { nameof(RuleResources.SARIF1001_Default) })
        { }

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor.Id != null &&
                reportingDescriptor.Name != null &&
                reportingDescriptor.Id.Equals(reportingDescriptor.Name, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF1001_Default),
                    reportingDescriptor.Id);
            }
        }
    }
}
